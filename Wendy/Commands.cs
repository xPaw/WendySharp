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

            RegisteredCommands = new List<Command>
            {
                new Kick(),
                new Ban(),
                new Unban(),
                new Redirect(),
                new Op(),
                new Deop(),
                new Voice(),
                new Devoice(),
                new Bans(),
                new Flex(),
                new Topic(),
                new TopicAppend(),
                new TopicPop(),
                new TopicReplace(),
                new TopicInsert(),
                new TopicRemove(),
                new Join(),
                new Part(),
                new Say(),
                new Quit(),
                new Moderated(),
                new Whoami(),
                new RedAlert(),
                new Emojic8(),
                new Help(this),
            };
            
            foreach (var cmd in RegisteredCommands)
            {
                cmd.Compile();
            }

            string pattern = @"^(?:" + string.Join("|", RegisteredCommands.Select(x => string.Format("({0})", string.Join("|", x.Match)))) + @")(?: |$)";

            CompiledCommandMatch = new Regex(pattern, RegexOptions.Compiled | RegexOptions.CultureInvariant);
        }

        public List<Command> GetRegisteredCommands()
        {
            return RegisteredCommands;
        }

        private void OnMessage(object obj, ChatMessageEventArgs e)
        {
            // Don't do anything in a private message
            if (!IrcValidation.IsChannelName(e.Recipient))
            {
                return;
            }

            var message = e.Message.ToString().TrimEnd();

            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            var isDirect = false;

            if (message.StartsWith(Bootstrap.Client.TrueNickname, StringComparison.InvariantCulture))
            {
                var length = Bootstrap.Client.TrueNickname.Length; // "Wendy: "

                // Allow pinging with any character following bots name
                if (message.Length < length + 2 || message[length] == ' ' || message[length + 1] != ' ')
                {
                    return;
                }

                message = message.Substring(length + 2);

                isDirect = true;
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

            Log.WriteDebug("CommandHandler", "Matched command '{0}' (as {2}) for {1}", command.Match.First(), e.Sender, match.Value);

            var arguments = new CommandArguments
            {
                IsDirect = isDirect,
                MatchedCommand = match.Value.Trim(),
                Event = e
            };
            
            if (command.Permission != null)
            {
                User user;

                // If there is no such user, don't pass
                if (!Users.TryGetUser(e.Sender, out user))
                {
                    Log.WriteDebug("CommandHandler", "'{0}' is not a user I know of, can't perform '{1}' ({2})", e.Sender, arguments.MatchedCommand, command.Permission);

                    return;
                }

                // If this user doesn't have required permission, don't pass
                if (!user.HasPermission(e.Recipient, command.Permission))
                {
                    Log.WriteDebug("CommandHandler", "'{0}' has no permission to perform '{1}' ({2})", e.Sender, arguments.MatchedCommand, command.Permission);

                    return;
                }

                Log.WriteDebug("CommandHandler", "'{0}' is authorized to perform '{1}' ({2})", e.Sender, arguments.MatchedCommand, command.Permission);
            }

            if (command.CompiledMatch != null)
            {
                arguments.Arguments = command.CompiledMatch.Match(message);

                if (!arguments.Arguments.Success)
                {
                    Help.PrintUsage(arguments, command);

                    return;
                }
            }

            command.OnCommand(arguments);
        }
    }
}
