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
            Usage = "<nick or hostmask> [for <duration>]";
            ArgumentMatch = "(?<nick>[^ ]+)(?: (?<duration>.+))?$"; // TODO: implement [reason]
            HelpText = "Bans or quiets a user.";
            Permission = "irc.op.ban";
        }

        public override void OnCommand(CommandArguments command)
        {
            // TODO: If we're not op, we should try to gain op
            var channel = Bootstrap.Client.ChannelList.GetChannel(command.Event.Recipient);

            if (!channel.WeAreOpped)
            {
                command.Reply("I'm not opped, send help.");

                return;
            }

            var nick = command.Arguments.Groups["nick"].Value;
            IrcIdentity ident;

            if (!IrcIdentity.TryParse(nick, out ident))
            {
                command.Reply("Invalid identity.");

                return;
            }

            var duration = command.Arguments.Groups["duration"].Value;
            DateTime durationTime = default(DateTime);

            if (duration.Length > 0)
            {
                command.Reply("Not yet implemented");

                return;
            }

            var isQuiet = command.MatchedCommand == "quiet" || command.MatchedCommand == "mute";

            Bootstrap.Client.Whois.Query(ident,
                whoisData =>
                {
                    if (whoisData.Identity.Nickname != null)
                    {
                        ident = whoisData.Identity;
                    }

                    var nickname = ident.Nickname;

                    if (nickname.ToString().ToLowerInvariant() == Bootstrap.Client.TrueNickname.ToLowerInvariant())
                    {
                        Log.WriteInfo("Ban", "{0} tried to ban the bot in {1}", command.Event.Sender, command.Event.Recipient);

                        command.Reply("Don't you even dare");

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

                    Log.WriteInfo("Ban", "{0} banned {1} from {2}", command.Event.Sender, ident, command.Event.Recipient);

                    var reason = string.Format("Banned by {0}", command.Event.Sender.Nickname);

                    Bootstrap.Client.Client.Mode(command.Event.Recipient, isQuiet ? "+q" : "+b", new IrcString[1] { ident });

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
                                Mode = isQuiet ? "-q" : "-b",
                                Time = durationTime,
                                Reason = reason
                            }
                        );
                    }
                    else
                    {
                        command.ReplyAsNotice("{0} {1}", isQuiet ? "muted" : "banned", ident);
                    }
                }
            );
        }
    }
}
