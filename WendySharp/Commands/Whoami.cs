﻿using System;
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

                    if (ident.Nickname.ToString().ToLowerInvariant() == Bootstrap.Client.TrueNickname.ToLowerInvariant())
                    {
                        command.Reply("That's me, dummy.");

                        return;
                    }

                    if (!Users.TryGetUser(ident, out var user))
                    {
                        command.Reply("This user has no permissions.");

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

                    if (!permissions.Any())
                    {
                        command.Reply("This user has no permissions in this channel.");

                        return;
                    }

                    command.Reply("{0} has following permissions in this channel: {1}", ident.Nickname, string.Join(", ", permissions.Distinct()));
                }
            );
        }
    }
}
