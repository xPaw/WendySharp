using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LitJson;
using NetIrc2;
using NetIrc2.Events;

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
        private Dictionary<string, SpamConfig> Channels;

        public Spam(IrcClient client)
        {
            Channels = new Dictionary<string, SpamConfig>();

            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config", "spam.json");

            if (File.Exists(path))
            {
                var data = File.ReadAllText(path);

                try
                {
                    Channels = JsonMapper.ToObject<Dictionary<string, SpamConfig>>(data);

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

            client.GotMessage += OnMessage;
            client.GotChatAction += OnMessage;
        }

        private void OnMessage(object obj, ChatMessageEventArgs e)
        {
            if (e.Sender == null || !Channels.ContainsKey(e.Recipient))
            {
                return;
            }

            User user;

            // If this user has a "spam.whitelist" permission, allow them to spam
            if (Users.TryGetUser(e.Sender, out user) && user.HasPermission(e.Recipient, "spam.whitelist"))
            {
                return;
            }

            var channel = Channels[e.Recipient];

            channel.AddAction(e.Sender, e.Message);

            var saidLines = channel.LastActions.Count(x =>
                x.Identity == e.Sender &&
                x.Time.AddSeconds(channel.LinesThresholdSeconds) >= DateTime.UtcNow
            );

            bool triggered = saidLines >= channel.LinesThreshold; // [triggering intensifies]

            if (!triggered)
            {
                var repeatLines = channel.LastActions.Count(x =>
                    x.Identity == e.Sender &&
                    x.Time.AddSeconds(channel.LinesThresholdSeconds) >= DateTime.UtcNow &&
                    x.Message == e.Message
                );

                triggered = repeatLines >= channel.RepeatThreshold;

                if (!triggered)
                {
                    return;
                }
            }

            channel.LastActions.Clear(); // TODO: FIX

            Log.WriteInfo("Spam", "A line by {0} in {1} was detected as spam. Quieting for {2} seconds.", e.Sender, e.Recipient, channel.Duration);

            var sender = e.Sender;
            sender.Nickname = "*";

            Bootstrap.Client.Client.Mode(e.Recipient, "+q", new IrcString[1] { sender });
            Bootstrap.Client.Client.Notice(e.Sender.Nickname, channel.Message);

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
    }
}
