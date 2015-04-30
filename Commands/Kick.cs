using System;
using NetIrc2;
using NetIrc2.Parsing;

namespace WendySharp
{
    class Kick : Command
    {
        public Kick()
        {
            Name = "kick";
            Match = "kick|gtfo|remove";
            Usage = "<nickname> [reason]";
            ArgumentMatch = "(?<nick>[^ ]+)( (?<reason>.*))?$";
            HelpText = "Kicks a user from the current channel";
            Permission = "irc.op.kick";
        }

        public override void OnCommand(CommandArguments command)
        {
            if (!IrcValidation.IsChannelName(command.Event.Recipient))
            {
                command.Reply("Can't kick here, silly.");

                return;
            }

            // TODO: If we're not op, we should try to gain op

            var nick = command.Arguments.Groups["nick"].Value;
            IrcIdentity ident;

            if (!IrcIdentity.TryParse(nick, out ident))
            {
                command.Reply("Invalid identity.");

                return;
            }

            if (ident.Nickname.ToString().ToLowerInvariant() == Bootstrap.Client.TrueNickname.ToLowerInvariant())
            {
                Log.WriteInfo("Kick", "{0} tried to kick the bot in {1}", command.Event.Sender, command.Event.Recipient);

                command.Reply("Don't you even dare");

                return;
            }

            var reason = command.Arguments.Groups["reason"].Value.Trim();

            Log.WriteInfo("Kick", "{0} kicked {1} in {2} (reason: {3})", command.Event.Sender, ident, command.Event.Recipient, reason.Length == 0 ? "no reason given" : reason);

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
