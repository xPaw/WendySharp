﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NetIrc2;
using NetIrc2.Events;
using Newtonsoft.Json;

namespace WendySharp
{
    /// <summary>
    /// A plugin that watches the configured channel for spammers/flooders.
    /// 
    /// The algorithm is pretty simple: if a user says more than a certain number
    /// of lines within a certain number of seconds, they are quieted temporarily.
    /// The values for number of lines and seconds are configurable. Also, tracked
    /// is lines that are repeats, with a separate configurable values.
    /// </summary>
    class Spam
    {
        private readonly Dictionary<string, SpamConfig> Channels;

        public Spam(IrcClient client)
        {
            Channels = new Dictionary<string, SpamConfig>();

            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config", "spam.json");

            if (File.Exists(path))
            {
                var data = File.ReadAllText(path);

                try
                {
                    Channels = JsonConvert.DeserializeObject<Dictionary<string, SpamConfig>>(data, new JsonSerializerSettings { MissingMemberHandling = MissingMemberHandling.Error });

                    foreach(var channel in Channels)
                    {
                        channel.Value.LastActions.Limit = Math.Max(channel.Value.LinesThreshold, channel.Value.RepeatThreshold) * 2;
                    }
                }
                catch (JsonException e)
                {
                    Log.WriteError("Spam", "Failed to parse spam.json file: {0}", e.Message);

                    Environment.Exit(1);
                }
            }
            else
            {
                Log.WriteWarn("Spam", "File config/spam.json doesn't exist");
            }

            client.GotLeaveChannel += OnLeaveChannel;
            client.GotUserQuit += OnUserQuit;
        }

        public void OnMessage(ChatMessageEventArgs e, string message, User user)
        {
            if (e.Sender == null || !Channels.ContainsKey(e.Recipient) || (user?.HasPermission(e.Recipient, "spam.whitelist") == true))
            {
                return;
            }

            var sender = e.Sender;
            var channel = Channels[e.Recipient];

            channel.AddAction(sender, message);

            var actualChannel = Bootstrap.Client.ChannelList.GetChannel(e.Recipient);
            var mentions = message.Split(' ').Count(word => word.Length > 2 && actualChannel.HasUser(word));

            if (mentions >= channel.UserMentionsInOneMessage)
            {
                Bootstrap.Client.Client.Notice(sender.Nickname, "Don't mention too many users at once.");
            }
            else
            {
                var saidLines = 0;
                var repeatLines = 0;

                foreach (var action in channel.LastActions)
                {
                    if (action.Identity != sender)
                    {
                        continue;
                    }

                    if (action.Time.AddSeconds(channel.LinesThresholdSeconds) >= DateTime.UtcNow)
                    {
                        saidLines++;
                    }

                    if (action.Time.AddSeconds(channel.RepeatThresholdSeconds) >= DateTime.UtcNow && action.Message == message)
                    {
                        repeatLines++;
                    }
                }

                if (saidLines < channel.LinesThreshold && repeatLines < channel.RepeatThreshold)
                {
                    return;
                }

                channel.LastActions.Clear(); // TODO: FIX

                Bootstrap.Client.Client.Notice(sender.Nickname, channel.Message);
            }

            Log.WriteInfo("Spam", "A line by '{0}' in {1} was detected as spam. Quieting for {2} seconds.", sender, e.Recipient, channel.Duration);

            sender = Whois.NormalizeIdentity(sender);

            Bootstrap.Client.Client.Mode(e.Recipient, "+q", sender);

            if (Bootstrap.Client.ModeList.Find(e.Recipient, sender.ToString(), "-q") != null)
            {
                Log.WriteInfo("Spam", "There's already a latemode set for '{0}' in {1}", sender, e.Recipient);

                return;
            }

            Bootstrap.Client.ModeList.AddLateModeRequest(
                new LateModeRequest
                {
                    Channel = e.Recipient,
                    Recipient = sender.ToString(),
                    Mode = "-q",
                    Time = DateTime.UtcNow.AddSeconds(channel.Duration),
                    Reason = "Spam"
                }
            );
        }

        private void OnLeaveChannel(object sender, JoinLeaveEventArgs e)
        {
            var channels = e.GetChannelList();

            foreach (var channel in channels)
            {
                if (!Channels.ContainsKey(channel) || IsWhitelisted(e.Identity, channel))
                {
                    continue;
                }

                ProcessQuit(e.Identity, channel, Channels[channel]);
            }
        }

        private void OnUserQuit(object sender, QuitEventArgs e)
        {
            foreach (var channel in Channels)
            {
                if (!Bootstrap.Client.ChannelList.GetChannel(channel.Key).HasUser(e.Identity.Nickname) || IsWhitelisted(e.Identity, channel.Key))
                {
                    continue;
                }

                ProcessQuit(e.Identity, channel.Key, channel.Value);
            }
        }

        private void ProcessQuit(IrcIdentity ident, string channelName, SpamConfig channel)
        {
            var nickname = ident.Nickname;

            ident = Whois.NormalizeIdentity(ident);

            var quits = channel.AddUserPart(ident.ToString(), channel.QuitsThresholdSeconds);

            if (quits < channel.QuitsThreshold)
            {
                return;
            }

            var banMinutes = channel.AddUserBan(ident.ToString()) * channel.QuitsBanMinutes;

            Log.WriteInfo("Spam", "'{1}' ({0}) is spamming joins/quits in {2}. Redirecting for {3} minutes.", nickname, ident, channelName, banMinutes);

            Bootstrap.Client.Client.Mode(channelName, "+b", ident + "$" + Bootstrap.Client.Settings.RedirectChannel);

            // In case they manage to come back before ban takes place
            Bootstrap.Client.Client.Kick(nickname, channelName, $"Fix your connection. Banned for {banMinutes} minutes");

            Bootstrap.Client.Client.Notice(nickname, $"Please fix your connection. You have been banned from {channelName} for {banMinutes} minutes for rapidly rejoining the channel.");

            if (Bootstrap.Client.ModeList.Find(channelName, ident.ToString(), "-b") != null)
            {
                Log.WriteInfo("Spam", "There's already a latemode set for '{0}' in {1}", ident, channelName);

                return;
            }

            Bootstrap.Client.ModeList.AddLateModeRequest(
                new LateModeRequest
                {
                    Channel = channelName,
                    Recipient = ident.ToString(),
                    Mode = "-b",
                    Time = DateTime.UtcNow.AddMinutes(banMinutes),
                    Reason = "Quit/leave flood"
                }
            );
        }

        private static bool IsWhitelisted(IrcIdentity ident, string channel)
        {
            // If this user has a "spam.whitelist" permission, allow them to spam
            return Users.TryGetUser(ident, out var user) && user.HasPermission(channel, "spam.whitelist");
        }
    }
}
