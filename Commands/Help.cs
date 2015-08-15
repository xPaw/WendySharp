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

            Match = new List<string>
            {
                "help"
            };
            Usage = "<command>";
            ArgumentMatch = "(?<command>.+)?$";
            HelpText = "Help on the help command. Displays a helpful help message about help, helps you help yourself use help. Helpful, huh?";
        }

        public override void OnCommand(CommandArguments command)
        {
            User user;
            Users.TryGetUser(command.Event.Sender, out user);

            var commands = Reference
                .GetRegisteredCommands()
                .Where(x => x.Permission == null || (user != null && user.HasPermission(command.Event.Recipient, x.Permission)));

            if (!commands.Any())
            {
                return;
            }

            var matchedCommand = command.Arguments.Groups["command"].Value;

            if (matchedCommand.Length == 0)
            {
                var allowedCommands = commands.Select(x => x.Match.First());

                command.ReplyAsNotice("Commands you have access to in this channel: {0}", string.Join(", ", allowedCommands));

                return;
            }

            var foundCommand = commands.FirstOrDefault(x => x.Match.Contains(matchedCommand));

            if (foundCommand == null)
            {
                command.ReplyAsNotice("That's not a command that I know of.");

                return;
            }

            PrintUsage(command, foundCommand, true);
        }

        public static void PrintUsage(CommandArguments commandArguments, Command command, bool printAliases = false)
        {
            commandArguments.ReplyAsNotice("Usage: {0} {1}", command.Match.First(), command.Usage ?? "(no arguments)");

            if (printAliases && command.Match.Count > 1)
            {
                commandArguments.ReplyAsNotice("Aliases: {0}", string.Join(", ", command.Match.Skip(1)));
            }

            commandArguments.ReplyAsNotice("{0}", command.HelpText ?? "No documentation provided (you're on your own!)");
        }
    }
}
