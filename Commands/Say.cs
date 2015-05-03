using System;
using System.Collections.Generic;
using NetIrc2.Parsing;

namespace WendySharp
{
    class Say : Command
    {
        public Say()
        {
            Match = new List<string>
            {
                "say",
                "echo"
            };
            Usage = "<#channel|nick> <text>";
            ArgumentMatch = "(?<target>[^ ]+) (?<text>.+)$";
            HelpText = "Send a message to the specified channel or nickname.";
            Permission = "irc.op.say";
        }

        public override void OnCommand(CommandArguments command)
        {
            var target = command.Arguments.Groups["target"].Value;
            var text = command.Arguments.Groups["text"].Value;

            if (!IrcValidation.IsChannelName(target) && !IrcValidation.IsNickname(target))
            {
                command.Reply("That doesn't look like a valid channel or nickname.");

                return;
            }

            Log.WriteInfo("Say", "'{0}' said to '{1}': '{2}'", command.Event.Sender, target, text);

            Bootstrap.Client.Client.Message(target, text);
        }
    }
}
