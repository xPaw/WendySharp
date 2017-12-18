using System;
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
            ArgumentMatch = "(?<pos>[0-9]+)$";
            HelpText = "Removes the pos'th topic selection. Remember indexes start at 0.";
            Permission = "irc.op.topic";
        }

        public override void OnCommand(CommandArguments command)
        {
            command.Reply("NYI");
        }
    }
}
