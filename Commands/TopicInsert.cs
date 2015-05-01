using System;
using NetIrc2;
using System.Collections.Generic;

namespace WendySharp
{
    class TopicInsert : Command
    {
        public TopicInsert()
        {
            Match = new List<string>
            {
                "topicinsert",
                "tinsert",
            };
            Usage = "<pos> <text>";
            ArgumentMatch = "(?<pos>-?\\d+) (?<text>.+)$";
            HelpText = "Set a topic in a channel.";
            Permission = "irc.op.topic";
        }

        public override void OnCommand(CommandArguments command)
        {
            command.Reply("NYI");
        }
    }
}
