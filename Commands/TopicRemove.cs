using System;
using NetIrc2;
using System.Collections.Generic;

namespace WendySharp
{
    class TopicRemove : Command
    {
        public TopicRemove()
        {
            Match = new List<string>
            {
                "topicremove",
                "tremove",
            };
            Usage = "<pos>";
            ArgumentMatch = "(?<pos>-?\\d+)$";
            HelpText = "Removes the pos'th topic selection.";
            Permission = "irc.op.topic";
        }

        public override void OnCommand(CommandArguments command)
        {
            command.Reply("NYI");
        }
    }
}
