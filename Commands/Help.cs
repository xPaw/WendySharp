using System;
using System.Collections.Generic;
using System.Linq;

namespace WendySharp
{
    class Help : Command
    {
        private readonly List<Command> RegisteredCommands; // TODO: duplication?

        public Help(List<Command> commands)
        {
            RegisteredCommands = commands;

            Name = "help";
            HelpText = "Help on the help command. Displays a helpful help message about help, helps you help yourself use help. Helpful, huh?";
        }

        public override void OnCommand(CommandArguments command)
        {
            command.Reply(true, "Commands you have access to: {0}", string.Join(", ", RegisteredCommands.Select(x => x.Name)));
        }
    }
}
