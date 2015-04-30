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
            client.GotMessage += OnMessage;

            RegisteredCommands = new List<Command>();
            RegisteredCommands.Add(new Kick());
            RegisteredCommands.Add(new Ban());
            RegisteredCommands.Add(new Unban());
            RegisteredCommands.Add(new Redirect());
            RegisteredCommands.Add(new Op());
            RegisteredCommands.Add(new Deop());
            RegisteredCommands.Add(new Voice());
            RegisteredCommands.Add(new Devoice());
            RegisteredCommands.Add(new Bans());
            RegisteredCommands.Add(new Join());
            RegisteredCommands.Add(new Part());
            RegisteredCommands.Add(new Echo());
            RegisteredCommands.Add(new Quit());
            RegisteredCommands.Add(new Help(RegisteredCommands));

            foreach (var cmd in RegisteredCommands)
            {
                cmd.Compile();
            }

            string pattern = @"^(?:" + string.Join("|", RegisteredCommands.Select(x => string.Format("({0})", x.Match ?? x.Name))) + @")(?: |$)";

            Log.WriteDebug("Commands", "Match: {0}", pattern);

            CompiledCommandMatch = new Regex(pattern, RegexOptions.Compiled | RegexOptions.CultureInvariant);
        }

        private void OnMessage(object obj, ChatMessageEventArgs e)
        {
            if (e.Sender == null)
            {
                return;
            }

            User user;

            // If there is no such user, don't do anything
            if (!Users.TryGetUser(e.Sender, out user))
            {
                return;
            }

            // Don't do anything in a private message
            if (!IrcValidation.IsChannelName(e.Recipient))
            {
                return;
            }

            var message = e.Message.ToString().TrimEnd();

            if (message.StartsWith(Bootstrap.Client.TrueNickname, StringComparison.InvariantCultureIgnoreCase))
            {
                var length = Bootstrap.Client.TrueNickname.Length; // "Wendy: "

                // Allow pinging with any character following bots name
                if (message.Length < length + 2 || message[length] == ' ' || message[length + 1] != ' ')
                {
                    return;
                }

                message = message.Substring(length + 2);
            }
            else if (message[0] == Bootstrap.Client.Settings.Prefix)
            {
                message = message.Substring(1);
            }
            else
            {
                return;
            }

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
                MatchedCommand = match.Value.Trim(),
                Event = e
            };
            
            if (command.Permission != null)
            {
                if (!user.HasPermission(e.Recipient, command.Permission))
                {
                    return;
                }

                Log.WriteDebug("CommandHandler", "'{0}' is authorized to perform '{1}' ({2})", e.Sender, command.Name, command.Permission);
            }

            if (command.CompiledMatch != null)
            {
                arguments.Arguments = command.CompiledMatch.Match(message);

                if (!arguments.Arguments.Success)
                {
                    // TODO: This will print usage to users that don't have access to this command
                    arguments.ReplyAsNotice("Usage: {0} {1}", command.Name, command.Usage);
                    arguments.ReplyAsNotice("{0}", command.HelpText);

                    return;
                }
            }

            command.OnCommand(arguments);
        }
    }
}
