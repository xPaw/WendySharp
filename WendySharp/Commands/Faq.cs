using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace WendySharp
{
    class Faq
    {
        private readonly Dictionary<string, Dictionary<string, string>> Commands;
        private readonly string FilePath;
        private const string Permission = "irc.op.faq";

        public Faq()
        {
            FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config", "faq.json");

            if (File.Exists(FilePath))
            {
                Commands = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(File.ReadAllText(FilePath));
            }
            else
            {
                Commands = new Dictionary<string, Dictionary<string, string>>();
            }
        }

        public void OnCommand(CommandArguments command)
        {
            // Trim first space after ?? if there is one
            if (command.MatchedCommand.Length > 1 && command.MatchedCommand[0] == ' ' && command.MatchedCommand[1] != ' ')
            {
                command.MatchedCommand = command.MatchedCommand.Substring(1);
            }
            
            var cmd = command.MatchedCommand.Split(' ');
            var trigger = cmd[0].ToLowerInvariant();

            if (trigger == "add" || trigger == "remove")
            {
                HandleModification(command, cmd, trigger == "remove");

                return;
            }

            if (trigger == "list")
            {
                if (!Commands.TryGetValue(command.Event.Recipient, out var faqEntries) || !faqEntries.Any())
                {
                    command.Reply("There are no FAQ entries in this channel.");

                    return;
                }

                command.Reply($"Available FAQ entries: {string.Join(", ", faqEntries.Keys)}");

                return;
            }

            if (!Commands.TryGetValue(command.Event.Recipient, out var channelCommands))
            {
                return;
            }

            if (!channelCommands.TryGetValue(trigger, out var text))
            {
                return;
            }

            // Replace reply target if specified
            if (cmd.Length > 1 && cmd[1].Length > 0)
            {
                command.Event.Sender.Nickname = $"{Color.RED}{cmd[1]}{Color.NORMAL}";
            }

            command.Reply(text);
        }

        private void HandleModification(CommandArguments command, IReadOnlyList<string> cmd, bool isRemoval)
        {
            if (command.User?.HasPermission(command.Event.Recipient, Permission) != true)
            {
                command.Reply("You have no permission to use this command.");

                return;
            }

            if (!Commands.ContainsKey(command.Event.Recipient))
            {
                Commands[command.Event.Recipient] = new Dictionary<string, string>();
            }

            string trigger;
            
            if (isRemoval)
            {
                if (cmd.Count < 2)
                {
                    command.Reply("Usage: ??remove <existing key>");

                    return;
                }

                trigger = cmd[1].ToLowerInvariant();
                
                if (!Commands[command.Event.Recipient].ContainsKey(trigger))
                {
                    command.Reply($"No such FAQ entry: {trigger}");

                    return;
                }
                
                Commands[command.Event.Recipient].Remove(trigger);

                command.Reply($"Removed FAQ entry: {trigger}");

                SaveToFile();

                return;
            }

            if (cmd.Count < 3)
            {
                command.Reply("Usage: ??add <key> <value>");

                return;
            }

            trigger = cmd[1].ToLowerInvariant();

            if (trigger == "list" || trigger == "add" || trigger == "remove")
            {
                command.Reply("You can not add a reserved key word as a FAQ entry.");

                return;
            }
            
            command.Reply("{1} FAQ entry: {0}", trigger, Commands[command.Event.Recipient].ContainsKey(trigger) ? "Modified" : "Added");

            Commands[command.Event.Recipient][trigger] = cmd[2];

            SaveToFile();
        }

        private void SaveToFile()
        {
            var json = JsonConvert.SerializeObject(Commands, new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            });

            File.WriteAllText(FilePath, json);
        }
    }
}
