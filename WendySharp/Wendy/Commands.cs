﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NetIrc2;
using NetIrc2.Events;

namespace WendySharp
{
    class Commands
    {
        private readonly List<Command> RegisteredCommands;
        private readonly Regex CompiledCommandMatch;
        private readonly Faq FaqCommand;
        private readonly Spam SpamPlugin;

        public Commands(IrcClient client)
        {
            client.GotMessage += OnMessage;
            client.GotChatAction += OnMessage;

            SpamPlugin = new Spam(client);
            FaqCommand = new Faq();

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
                new Parrot(),
                new Say(),
                new Moderated(),
                new Whoami(),
                new Help(this),
            };

            foreach (var cmd in RegisteredCommands)
            {
                cmd.Compile();
            }

            var pattern = "^(?:" + string.Join("|", RegisteredCommands.Select(x => $"({string.Join("|", x.Match)})")) + ")(?: |$)";

            CompiledCommandMatch = new Regex(pattern, RegexOptions.Compiled | RegexOptions.CultureInvariant);
        }

        public IEnumerable<Command> GetRegisteredCommands()
        {
            return RegisteredCommands;
        }

        private void OnMessage(object obj, ChatMessageEventArgs e)
        {
            // Don't do anything in a private message
            if (e.Recipient[0] != '#')
            {
                return;
            }

            var message = e.Message.ToString().TrimEnd();
            Users.TryGetUser(e.Sender, out var user);

            SpamPlugin.OnMessage(e, message, user);

            if (message.Length < 2)
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
            else if (message[0] == Bootstrap.Client.Settings.Prefix && Bootstrap.Client.Settings.Channels.Contains(e.Recipient))
            {
                message = message.Substring(1);
            }
            else if (message[0] == '?' && message[1] == '?' && Bootstrap.Client.Settings.Channels.Contains(e.Recipient))
            {
                FaqCommand.OnCommand(new CommandArguments
                {
                    User = user,
                    MatchedCommand = message.Substring(2),
                    Event = e
                });

                return;
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

            var i = 1;

            while (match.Groups[i].Value.Length == 0)
            {
                i++;
            }

            var command = RegisteredCommands[i - 1];
            var matchedCommand = match.Value.Trim();

            Log.WriteInfo("CommandHandler", "Matched command '{0}' (as {1}) for {2}", command.Match[0], matchedCommand, e.Sender);

            var arguments = new CommandArguments
            {
                IsDirect = isDirect,
                User = user,
                MatchedCommand = matchedCommand,
                Event = e
            };

            if (command.Permission != null)
            {
                // If there is no such user, don't pass
                if (user == null)
                {
                    Log.WriteInfo("CommandHandler", "'{0}' is not a user I know of, can't perform '{1}' ({2})", e.Sender, arguments.MatchedCommand, command.Permission);

                    return;
                }

                // If this user doesn't have required permission, don't pass
                if (!user.HasPermission(e.Recipient, command.Permission))
                {
                    Log.WriteInfo("CommandHandler", "'{0}' has no permission to perform '{1}' ({2})", e.Sender, arguments.MatchedCommand, command.Permission);

                    return;
                }

                Log.WriteInfo("CommandHandler", "'{0}' is authorized to perform '{1}' ({2})", e.Sender, arguments.MatchedCommand, command.Permission);
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
