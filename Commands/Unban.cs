using System;
using NetIrc2;
using NetIrc2.Parsing;

namespace WendySharp
{
    class Unban : Command
    {
        public Unban()
        {
            Name = "unban";
            Usage = "<nick or hostmask>";
            ArgumentMatch = "(?<nick>[^ ]+)$"; // TODO: implement [reason]
            HelpText = "Un-bans a user";
            Permission = "irc.op.ban";
        }

        public override void OnCommand(CommandArguments command)
        {
            if (!IrcValidation.IsChannelName(command.Event.Recipient))
            {
                command.Reply("Can't unban here, silly.");

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

            if (ident.Hostname == null)
            {
                ident.Hostname = "*";
            }

            if (ident.Username == null)
            {
                ident.Username = "*";
            }

            Log.WriteInfo("Unban", "{0} unbanned {1} in {2}", command.Event.Sender, ident, command.Event.Recipient);

            Bootstrap.Client.Client.Mode(command.Event.Recipient, "-b", new IrcString[1] { ident });

            command.Reply("Unbanned {0}", ident);
        }
    }
}
