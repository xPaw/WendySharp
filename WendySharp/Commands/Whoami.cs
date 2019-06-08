using System;
using System.Collections.Generic;
using System.Linq;
using NetIrc2;

namespace WendySharp
{
    class Whoami : Command
    {
        public Whoami()
        {
            Match = new List<string>
            {
                "whoami"
            };
            Usage = "[nickname]";
            ArgumentMatch = "(?<nick>[^ ]+)?$";
            HelpText = "Lists permission in current channel for given user.";
        }

        public override void OnCommand(CommandArguments command)
        {
            var nick = command.Arguments.Groups["nick"].Value;

            if (nick.Length == 0 || !IrcIdentity.TryParse(nick, out var ident))
            {
                ident = command.Event.Sender;
            }

            Bootstrap.Client.Whois.Query(ident,
                whoisData =>
                {
                    if (whoisData.Identity.Nickname == null)
                    {
                        command.Reply("There is no user by that nick on the network.");

                        return;
                    }

                    ident = whoisData.Identity;

                    if (string.Equals(ident.Nickname.ToString(), Bootstrap.Client.TrueNickname, StringComparison.InvariantCultureIgnoreCase))
                    {
                        command.Reply("That's me, dummy.");

                        return;
                    }

                    if (!Users.TryGetUser(ident, out var user))
                    {
                        if (ident.Nickname == command.Event.Sender)
                        {
                            command.Reply("You have no permissions.");

                            return;
                        }

                        command.Reply($"{ident.Nickname} has no permissions.");

                        return;
                    }

                    var permissions = new List<string>();

                    if (user.Permissions.ContainsKey("*"))
                    {
                        permissions.AddRange(user.Permissions["*"]);
                    }

                    if (user.Permissions.ContainsKey(command.Event.Recipient))
                    {
                        permissions.AddRange(user.Permissions[command.Event.Recipient]);
                    }

                    if (ident.Nickname == command.Event.Sender)
                    {
                        if (permissions.Count > 0)
                        {
                            command.Reply($"You have following permissions in this channel: {string.Join(", ", permissions.Distinct())}");

                            return;
                        }

                        command.Reply("You have no permissions in this channel.");

                        return;
                    }

                    if (permissions.Count > 0)
                    {
                        command.Reply($"{ident.Nickname} has following permissions in this channel: {string.Join(", ", permissions.Distinct())}");

                        return;
                    }

                    command.Reply($"{ident.Nickname} has no permissions in this channel.");
                }
            );
        }
    }
}
