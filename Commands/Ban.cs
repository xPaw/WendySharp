using System;
using NetIrc2;
using NetIrc2.Parsing;

namespace WendySharp
{
    class Ban : Command
    {
        public Ban()
        {
            Name = "ban";
            Match = "ban|kban|kb|kickban";
            Usage = "<nick or hostmask> [for <duration>]";
            ArgumentMatch = "(?<nick>[^ ]+)(?: (?<duration>.+))?$"; // TODO: implement [reason]
            HelpText = "Bans a user";
            Permission = "irc.op.ban";
        }

        public override void OnCommand(CommandArguments command)
        {
            if (!IrcValidation.IsChannelName(command.Event.Recipient))
            {
                command.Reply("Can't ban here, silly.");

                return;
            }

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

            // TODO: implement whois

            if (ident.Nickname.ToString().ToLowerInvariant() == Bootstrap.Client.TrueNickname.ToLowerInvariant())
            {
                Log.WriteInfo("Ban", "{0} tried to ban the bot in {1}", command.Event.Sender, command.Event.Recipient);

                command.Reply("Don't you even dare");

                return;
            }

            var duration = command.Arguments.Groups["duration"].Value;
            DateTime durationTime = default(DateTime);

            if (ident.Hostname == null)
            {
                ident.Hostname = "*";
            }

            if (ident.Username == null)
            {
                ident.Username = "*";
            }

            if (Bootstrap.Client.ModeList.Find(command.Event.Recipient, ident.ToString(), "-b") != null)
            {
                command.Reply("{0} is already banned in this channel.", ident);

                return;
            }

            Log.WriteInfo("Ban", "{0} banned {1} from {2}", command.Event.Sender, ident, command.Event.Recipient);

            var isNickInChannel = channel.HasUser(ident.Nickname);
            var reason = string.Format("Banned by {0}", command.Event.Sender.Nickname);

            if (duration.Length > 0)
            {
                // TODO: get some parsedatetime

                durationTime = DateTime.UtcNow.AddMinutes(1);
            }

            Bootstrap.Client.Client.Mode(command.Event.Recipient, "+b", new IrcString[1] { ident });
            Bootstrap.Client.Client.Kick(ident.Nickname, command.Event.Recipient, reason);

            command.Reply("Will unban {0} {1}{2}",
                ident,
                durationTime == default(DateTime) ? "never" : durationTime.ToRelativeString(),
                isNickInChannel ? "" : " (this nick doesn't appear to be in this channel)"
            );

            Bootstrap.Client.ModeList.AddLateModeRequest(
                new LateModeRequest
                {
                    Channel = command.Event.Recipient,
                    Recipient = ident.ToString(),
                    Mode = "-b",
                    Time = durationTime,
                    Reason = reason
                }
            );
        }
    }
}
