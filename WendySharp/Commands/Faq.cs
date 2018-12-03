using System;
using System.Collections.Generic;
using System.IO;
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
            Commands = new Dictionary<string, Dictionary<string, string>>();

            FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config", "faq.json");

            if (File.Exists(FilePath))
            {
                Commands = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(File.ReadAllText(FilePath));
            }
        }

        public void OnCommand(CommandArguments command)
        {
            var cmd = command.MatchedCommand.Split(' ');

            if (cmd[0] == "add" || cmd[0] == "remove")
            {
                HandleModification(command, cmd, cmd[0] == "remove");

                return;
            }

            if (cmd[0] == "list")
            {
                if (!Commands.ContainsKey(command.Event.Recipient))
                {
                    command.Reply($"There are no commands in this channel.");
                }

                command.Reply($"Available commands: {string.Join(", ", Commands[command.Event.Recipient].Keys)}");

                return;
            }

            if (!Commands.ContainsKey(command.Event.Recipient))
            {
                return;
            }

            if (!Commands[command.Event.Recipient].TryGetValue(cmd[0], out var text))
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
            if (command.User == null || !command.User.HasPermission(command.Event.Recipient, Permission))
            {
                command.Reply("You have no permission to use this command.");

                return;
            }

            if (!Commands.ContainsKey(command.Event.Recipient))
            {
                Commands[command.Event.Recipient] = new Dictionary<string, string>();
            }

            if (isRemoval)
            {
                if (cmd.Count < 2 || Commands[command.Event.Recipient].ContainsKey(cmd[1]))
                {
                    command.Reply("Usage: ??remove <existing key>");
                }

                Commands[command.Event.Recipient].Remove(cmd[1]);

                command.Reply("Removed '{0}'", cmd[1]);

                SaveToFile();

                return;
            }

            if (cmd.Count < 3)
            {
                command.Reply("Usage: ??add <key> <value>");

                return;
            }

            command.Reply("{1} '{0}'", cmd[1], Commands[command.Event.Recipient].ContainsKey(cmd[1]) ? "Modified" : "Added");

            Commands[command.Event.Recipient][cmd[1]] = cmd[2];

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
