using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NetIrc2;
using NetIrc2.Events;
using NetIrc2.Parsing;

namespace WendySharp
{
    class Commands
    {
        private readonly List<Command> RegisteredCommands;
        private readonly Regex CompiledCommandMatch;

        public Commands(IrcClient client)
        {
            client.GotMessage += OnIRCMessage;

            RegisteredCommands = new List<Command>();
            RegisteredCommands.Add(new Kick());
            RegisteredCommands.Add(new Join());
            RegisteredCommands.Add(new Part());
            RegisteredCommands.Add(new Echo());
            RegisteredCommands.Add(new Quit());
            RegisteredCommands.Add(new Help(RegisteredCommands));

            foreach (var cmd in RegisteredCommands)
            {
                cmd.Compile();
            }

            string pattern = @"^(?:" + string.Join("|", RegisteredCommands.Select(x => string.Format("({0})", x.Match ?? x.Name))) + @")";

            Log.WriteDebug("Commands", "Match: {0}", pattern);

            CompiledCommandMatch = new Regex(pattern, RegexOptions.Compiled | RegexOptions.CultureInvariant);
        }

        private void OnIRCMessage(object obj, ChatMessageEventArgs e)
        {
            if (e.Sender == null)
            {
                return;
            }

            Log.WriteDebug("CommandHandler", "{0} -> {1} : {2}", e.Sender, e.Recipient, e.Message);

            var message = e.Message.ToString().TrimEnd();

            if (message.StartsWith(Settings.BotNick, StringComparison.Ordinal))
            {
                var length = Settings.BotNick.Length; // "Wendy: "

                // Allow pinging with any character following bots name
                if (message.Length < length + 2 || message[length] == ' ' || message[length + 1] != ' ')
                {
                    return;
                }

                message = message.Substring(length + 2);
            }
            else if (message[0] == Settings.BotPrefix)
            {
                message = message.Substring(1);
            }
            else if(!IrcValidation.IsChannelName(e.Recipient))
            {
                // Direct messages don't need a prefix
            }
            else
            {
                return;
            }

            Log.WriteDebug("CommandHandler", "Checking for command '{0}'", message);

            var match = CompiledCommandMatch.Match(message);

            if (!match.Success)
            {
                return;
            }

            int i = 1;

            while (match.Groups[i].Value.Length == 0)
            {
                i++;
            }

            var command = RegisteredCommands[i - 1];

            Log.WriteDebug("CommandHandler", "Matched command '{0}' (as {2}) for {1}", command.Name, e.Sender, match.Value);

            var arguments = new CommandArguments
            {
                Event = e
            };

            if (command.Permission != null)
            {
                if (e.Sender.Username == null)
                {
                    arguments.Reply("This command needs permission '{0}', but you don't have a username (not authed).", command.Permission);

                    return;
                }

                Log.WriteDebug("CommandHandler", "Trying to find user '{0}'", e.Sender.Username);

                User user;

                if (!Users.TryGetUser(e.Sender, out user))
                {
                    arguments.Reply("This command needs permission '{0}', but you don't have any special permissions (not in users.json).", command.Permission);

                    return;
                }

                if (!user.HasPermission(e.Recipient, command.Permission))
                {
                    arguments.Reply("This command needs permission '{0}', but you don't have this permission in '{1}'.", command.Permission, e.Recipient);

                    return;
                }
            }

            if (command.CompiledMatch != null)
            {
                arguments.Arguments = command.CompiledMatch.Match(message);

                if (!arguments.Arguments.Success)
                {
                    arguments.Reply(true, "Usage: {0} {1}", command.Name, command.Usage);
                    arguments.Reply(true, "{0}", command.HelpText);

                    return;
                }
            }

            command.OnCommand(arguments);
        }
    }
}
