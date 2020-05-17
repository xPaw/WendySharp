﻿using System;
using System.Collections.Generic;
using NetIrc2;

namespace WendySharp
{
    class Deop : Command
    {
        public Deop()
        {
            Match = new List<string>
            {
                "deop"
            };
            Usage = "[nick] ...";
            ArgumentMatch = "(?<nicks>.+)?";
            HelpText = "Takes op from the specified user.";
            Permission = "irc.op.op";
        }

        public override void OnCommand(CommandArguments command)
        {
            var nicks = new List<IrcString>();
            var input = command.Arguments.Groups["nicks"].Value;

            if (input.Length == 0)
            {
                nicks.Add(command.Event.Sender.Nickname);
            }
            else
            {
                var nicksTemp = input.Split(' ');

                foreach (var nick in nicksTemp)
                {
                    if (!IrcIdentity.TryParse(nick, out var ident))
                    {
                        command.Reply("'{0}' is a invalid indentity.", nick);

                        return;
                    }

                    nicks.Add(ident.Nickname);
                }
            }

            var channel = Bootstrap.Client.ChannelList.GetChannel(command.Event.Recipient);

            if (channel.WeAreOpped)
            {
                Bootstrap.Client.Client.Mode(command.Event.Recipient, "-" + new string('o', nicks.Count), nicks.ToArray());
            }
            else if (channel.HasChanServ)
            {
                Bootstrap.Client.Client.IrcCommand("CHANSERV", "deop", channel.Name, string.Join(" ", nicks));
            }
            else
            {
                command.Reply("I'm not opped, send help.");

                return;
            }

            Log.WriteInfo("Deop", "'{0}' took channel operator from {1} in {2}", command.Event.Sender, string.Join(", ", nicks), command.Event.Recipient);
        }
    }
}
