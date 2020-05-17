﻿using System;
using System.Collections.Generic;
using NetIrc2;

namespace WendySharp
{
    class Op : Command
    {
        public Op()
        {
            Match = new List<string>
            {
                "op"
            };
            Usage = "[nick] ...";
            ArgumentMatch = "(?<nicks>.+)?";
            HelpText = "Gives op to the specified user.";
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
                Bootstrap.Client.Client.Mode(command.Event.Recipient, "+" + new string('o', nicks.Count), nicks.ToArray());
            }
            else if (channel.HasChanServ)
            {
                Bootstrap.Client.Client.IrcCommand("CHANSERV", "op", channel.Name, string.Join(" ", nicks));
            }
            else
            {
                command.Reply("I'm not opped, send help.");

                return;
            }

            Log.WriteInfo("Op", "'{0}' gave channel operator to {1} in {2}", command.Event.Sender, string.Join(", ", nicks), command.Event.Recipient);
        }
    }
}
