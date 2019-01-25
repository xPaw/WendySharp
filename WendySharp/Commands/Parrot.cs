﻿using System;
using System.Collections.Generic;

namespace WendySharp
{
    class Parrot : Command
    {
        public Parrot()
        {
            Match = new List<string>
            {
                "parrot"
            };
            Usage = "<text>";
            ArgumentMatch = "(?<text>.+)$";
            HelpText = "Echo a message back to the channel.";
        }

        public override void OnCommand(CommandArguments command)
        {
            var text = command.Arguments.Groups["text"].Value;

            if (text.Length > 140)
            {
                command.Reply("That message is too long to be parroted.");
                return;
            }

            Log.WriteInfo("Parrot", "'{0}' said to '{1}': {2}", command.Event.Sender, command.Event.Recipient, text);

            Bootstrap.Client.Client.Message(command.Event.Recipient, string.Format("\u200B{0}", text));
        }
    }
}
