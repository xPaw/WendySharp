﻿using System;
using System.Collections.Generic;
using NetIrc2;
using NetIrc2.Parsing;

namespace WendySharp
{
    class Devoice : Command
    {
        public Devoice()
        {
            Name = "devoice";
            Match = "devoice|dehat|unhat";
            Usage = "[nick] ...";
            ArgumentMatch = "(?<nicks>.+)?";
            HelpText = "Takes voice from the specified user";
            Permission = "irc.op.voice";
        }

        public override void OnCommand(CommandArguments command)
        {
            if (!IrcValidation.IsChannelName(command.Event.Recipient))
            {
                command.Reply("Can't devoice here, silly.");

                return;
            }

            var nicks = new List<string>();
            var input = command.Arguments.Groups["nicks"].Value;

            if (input.Length == 0)
            {
                nicks.Add(command.Event.Sender.Nickname);
            }
            else
            {
                var nicksTemp = input.Split(' ');
                IrcIdentity ident;

                foreach (var nick in nicksTemp)
                {
                    if (!IrcIdentity.TryParse(nick, out ident))
                    {
                        command.Reply("Invalid identity.");

                        return;
                    }

                    nicks.Add(ident.Nickname);
                }
            }

            var channel = Bootstrap.Client.ChannelList.GetChannel(command.Event.Recipient);

            foreach (var nick in nicks)
            {
                if (channel.WeAreOpped)
                {
                    Bootstrap.Client.Client.Mode(command.Event.Recipient, "-v", new IrcString[1] { nick });
                }
                else if(channel.HasUser("ChanServ"))
                {
                    Bootstrap.Client.Client.Message("ChanServ", string.Format("devoice {0} {1}", channel.Name, nick));
                }
                else
                {
                    command.Reply("I'm not opped, send help.");

                    return;
                }
            }

            Log.WriteInfo("Deop", "{0} took voice from {1} in {2}", command.Event.Sender, string.Join(", ", nicks), command.Event.Recipient);
        }
    }
}