using System.Collections.Generic;
using System.Linq;

namespace WendySharp
{
    class Help : Command
    {
        private readonly Commands Reference;

        public Help(Commands commands)
        {
            Reference = commands;

            Name = "help";
            HelpText = "Help on the help command. Displays a helpful help message about help, helps you help yourself use help. Helpful, huh?";
        }

        public override void OnCommand(CommandArguments command)
        {
            User user;
            Users.TryGetUser(command.Event.Sender, out user);

            var commands = Reference
                .GetRegisteredCommands()
                .Where(x => x.Permission == null || user.HasPermission(command.Event.Recipient, x.Permission))
                .Select(x => x.Name);

            command.ReplyAsNotice("Commands you have access to in this channel: {0}", string.Join(", ", commands));
        }
    }
}
