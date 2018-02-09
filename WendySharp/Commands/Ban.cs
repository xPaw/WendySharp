using System;
using System.Collections.Generic;
using NetIrc2;

namespace WendySharp
{
    class Ban : Command
    {
        public Ban()
        {
            Match = new List<string>
            {
                "ban",
                "kban",
                "kb",
                "kickban",
                "quiet",
                "mute",
            };
            Usage = "<nick or hostmask> [<duration>mi|h|d|w|mo] [reason]";
            ArgumentMatch = "(?<nick>[^ ]+)( (?<duration>[0-9]+)(?<durationUnit>[a-z]+))?( (?<reason>.*))?$";
            HelpText = "Bans or quiets a user.";
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

            var duration = command.Arguments.Groups["duration"].Value;
            var durationTime = default(DateTime);

            if (duration.Length > 0)
            {
                try
                {
                    durationTime = DateTimeParser.Parse(duration, command.Arguments.Groups["durationUnit"].Value);
                }
                catch (ArgumentException e)
                {
                    command.Reply("{0}", e.Message);

                    return;
                }
            }

            var channel = Bootstrap.Client.ChannelList.GetChannel(command.Event.Recipient);

            if (!channel.WeAreOpped)
            {
                if (channel.HasChanServ)
                {
                    Bootstrap.Client.Client.Message("ChanServ", string.Format("op {0}", channel.Name));
                }
                else
                {
                    command.Reply("I'm not opped, send help.");

                    return;
                }
            }

            var isQuiet = command.MatchedCommand == "quiet" || command.MatchedCommand == "mute";

            Bootstrap.Client.Whois.Query(ident,
                whoisData =>
                {
                    if (whoisData.Identity.Nickname == null)
                    {
                        command.Reply("There is no user by that nick on the network. Try {0}!*@* to {1} anyone with that nick, or specify a full hostmask.", ident.Nickname, isQuiet ? "quiet" : "ban");

                        return;
                    }

                    ident = whoisData.Identity;

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

                    if (Bootstrap.Client.ModeList.Find(command.Event.Recipient, ident.ToString(), isQuiet ? "-q" : "-b") != null)
                    {
                        command.Reply("{0} is already {1} in this channel.", ident, isQuiet ? "muted" : "banned");

                        return;
                    }

                    Log.WriteInfo("Ban", "'{0}' {1} '{2}' from {3}", command.Event.Sender, isQuiet ? "quieted" : "banned", ident, command.Event.Recipient);

                    var reason = command.Arguments.Groups["reason"].Value.Trim();

                    if(reason.Length == 0)
                    {
                        reason = string.Format("Banned by {0}", command.Event.Sender.Nickname);
                    }

                    Bootstrap.Client.Client.Mode(command.Event.Recipient, isQuiet ? "+q" : "+b", ident);

                    if (!isQuiet && channel.HasUser(nickname))
                    {
                        Bootstrap.Client.Client.Kick(nickname, command.Event.Recipient, reason);
                    }

                    if (duration.Length > 0)
                    {
                        command.ReplyAsNotice("Will {0} {1} {2}", isQuiet ? "unmute" : "unban", ident, durationTime.ToRelativeString());

                        Bootstrap.Client.ModeList.AddLateModeRequest(
                            new LateModeRequest
                            {
                                Channel = command.Event.Recipient,
                                Recipient = ident.ToString(),
                                Sender = command.Event.Sender.ToString(),
                                Mode = isQuiet ? "-q" : "-b",
                                Time = durationTime,
                                Reason = reason
                            }
                        );
                    }
                    else
                    {
                        command.ReplyAsNotice("{0} {1}", isQuiet ? "Muted" : "Banned", ident);
                    }
                }
            );
        }
    }
}
