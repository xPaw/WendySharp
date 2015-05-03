using System;
using System.Collections.Generic;
using NetIrc2;

namespace WendySharp
{
    class Kick : Command
    {
        public Kick()
        {
            Match = new List<string>
            {
                "kick",
                "gtfo",
                "remove"
            };
            Usage = "<nickname> [reason]";
            ArgumentMatch = "(?<nick>[^ ]+)( (?<reason>.*))?$";
            HelpText = "Kicks a user from the current channel.";
            Permission = "irc.op.kick";
        }

        public override void OnCommand(CommandArguments command)
        {
            var nick = command.Arguments.Groups["nick"].Value;
            IrcIdentity ident;

            if (!IrcIdentity.TryParse(nick, out ident))
            {
                command.Reply("Invalid identity.");

                return;
            }

            if (ident.Nickname.ToString().ToLowerInvariant() == Bootstrap.Client.TrueNickname.ToLowerInvariant())
            {
                command.Reply("Don't you even dare.");

                return;
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

            var reason = command.Arguments.Groups["reason"].Value.Trim();

            Log.WriteInfo("Kick", "'{0}' kicked '{1}' in {2} (reason: {3})", command.Event.Sender, ident, command.Event.Recipient, reason.Length == 0 ? "no reason given" : reason);

            if (command.MatchedCommand == "remove")
            {
                Bootstrap.Client.Client.Remove(ident.Nickname, command.Event.Recipient, reason.Length == 0 ? null : reason);
            }
            else
            {
                Bootstrap.Client.Client.Kick(ident.Nickname, command.Event.Recipient, reason.Length == 0 ? null : reason);
            }

            command.ReplyAsNotice("Kicked {0}", ident);
        }
    }
}
