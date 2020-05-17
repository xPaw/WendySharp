using System;
using System.Collections.Generic;
using NetIrc2;
using NetIrc2.Parsing;

namespace WendySharp
{
    class Redirect : Command
    {
        public Redirect()
        {
            Match = new List<string>
            {
                "redirect",
                "fixurshit",
                "fixurconnection",
                "fixyourshit",
                "fixyourconnection",
            };
            Usage = "<nickname> [channel]";
            ArgumentMatch = "(?<nick>[^ ]+)(?: (?<channel>[^ ]+))?";
            HelpText = "Redirects a user to ##FIX_YOUR_CONNECTION or the given channel for a hard-coded length of time (2 hours).";
            Permission = "irc.op.ban";
        }

        public override void OnCommand(CommandArguments command)
        {
            var nick = command.Arguments.Groups["nick"].Value;

            if (!IrcIdentity.TryParse(nick, out var ident))
            {
                command.Reply("Invalid identity.");

                return;
            }

            var channel = Bootstrap.Client.ChannelList.GetChannel(command.Event.Recipient);

            if (!channel.WeAreOpped)
            {
                if (channel.HasChanServ)
                {
                    Bootstrap.Client.Client.IrcCommand("CHANSERV", "op", channel.Name);
                }
                else
                {
                    command.Reply("I'm not opped, send help.");

                    return;
                }
            }

            Bootstrap.Client.Whois.Query(ident,
                whoisData =>
                {
                    if (whoisData.Identity.Nickname != null)
                    {
                        ident = whoisData.Identity;
                    }

                    var nickname = ident.Nickname;

                    if (string.Equals(nickname.ToString(), Bootstrap.Client.TrueNickname, StringComparison.InvariantCultureIgnoreCase))
                    {
                        command.Reply("Don't you even dare.");

                        return;
                    }

                    if (whoisData.Identity.Nickname != null)
                    {
                        Whois.NormalizeIdentity(ident);
                    }
                    else
                    {
                        if (ident.Username == null)
                        {
                            ident.Username = "*";
                        }

                        if (ident.Hostname == null)
                        {
                            ident.Hostname = "*";
                        }
                    }

                    var targetChannel = command.Arguments.Groups["channel"].Value.Trim();

                    if (targetChannel.Length == 0)
                    {
                        targetChannel = Bootstrap.Client.Settings.RedirectChannel;
                    }
                    else if (!IrcValidation.IsChannelName(targetChannel))
                    {
                        command.Reply("Invalid target channel.");

                        return;
                    }

                    if (Bootstrap.Client.ModeList.Find(command.Event.Recipient, ident.ToString(), "-b") != null)
                    {
                        command.Reply("{0} is already banned in this channel.", ident);

                        return;
                    }

                    Log.WriteInfo("Redirect", "'{0}' redirected '{1}' from {2} to {3}", command.Event.Sender, ident, command.Event.Recipient, targetChannel);

                    var reason = string.Format("Redirected to {0} by {1} for 2 hours", targetChannel, command.Event.Sender.Nickname);

                    Bootstrap.Client.Client.Mode(command.Event.Recipient, "+b", ident + "$" + targetChannel);

                    if (channel.HasUser(nickname))
                    {
                        Bootstrap.Client.Client.Kick(nickname, command.Event.Recipient, reason);
                    }

                    command.ReplyAsNotice("Redirected {0} to {1} for 2 hours", ident, targetChannel);

                    Bootstrap.Client.ModeList.AddLateModeRequest(
                        new LateModeRequest
                        {
                            Channel = command.Event.Recipient,
                            Recipient = ident.ToString(),
                            Sender = command.Event.Sender.ToString(),
                            Mode = "-b",
                            Time = DateTime.UtcNow.AddHours(2),
                            Reason = reason
                        }
                    );
                }
            );
        }
    }
}
