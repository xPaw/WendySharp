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
            ArgumentMatch = "(?<pos>[0-9]+) (?<text>.+)$";
            HelpText = "Inserts text into the topic at the given position. Remember indexes start at 0.";
            Permission = "irc.op.topic";
        }

        public override void OnCommand(CommandArguments command)
        {
            command.Reply("NYI");
        }
    }
}
