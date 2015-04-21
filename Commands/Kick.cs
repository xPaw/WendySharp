using System;
using NetIrc2;

namespace WendySharp
{
    class Kick : Command
    {
        public Kick()
        {
            Name = "kick";
            Match = "kick|KICK|gtfo";
            Usage = "<nickname> [reason]";
            ArgumentMatch = "(?<nick>[^ ]+)( (?<reason>.*))?$";
            HelpText = "Kicks a user from the current channel";
            Permission = "irc.op.kick";
        }

        public override void OnCommand(CommandArguments command)
        {
            if (!Bootstrap.IsRecipientChannel(command.Event.Recipient))
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

            if (ident.Nickname.ToString().ToLowerInvariant() == Settings.BotNick.ToLowerInvariant())
            {
                Log.WriteInfo("Kick", "{0} tried to kick the bot in {1}", command.Event.Sender, command.Event.Recipient);

                Bootstrap.Client.Client.Kick(ident.Nickname, command.Event.Recipient, "don't you even dare");

                return;
            }

            var reason = command.Arguments.Groups["reason"].Value.Trim();

            Log.WriteInfo("Kick", "{0} kicked {1} in {2} (reason: {3})", command.Event.Sender, ident, command.Event.Recipient, reason.Length == 0 ? "no reason given" : reason);

            Bootstrap.Client.Client.Kick(ident.Nickname, command.Event.Recipient, reason);
        }
    }
}
