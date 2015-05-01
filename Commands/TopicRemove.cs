using System;
using NetIrc2;

namespace WendySharp
{
    class TopicRemove : Command
    {
        public TopicRemove()
        {
            Name = "topicremove";
            Match = "topicremove|tremove";
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
