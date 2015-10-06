using System;
using System.Collections.Generic;
using System.Linq;

namespace WendySharp
{
    class TopicReplace : Command
    {
        public TopicReplace()
        {
            Match = new List<string>
            {
                "topicreplace",
                "treplace",
            };
            Usage = "<pos> <text>";
            ArgumentMatch = "(?<pos>[0-9]+) (?<text>.+)$";
            HelpText = "Replaces the given section with the given text. Remember indexes start at 0.";
            Permission = "irc.op.topic";
        }

        public override void OnCommand(CommandArguments command)
        {
            var channel = Bootstrap.Client.ChannelList.GetChannel(command.Event.Recipient);

            var topic = channel.Topic.Split(new [] { " | " }, StringSplitOptions.None);
            var count = topic.Count();

            if (count < 2)
            {
                command.Reply("Not enough parts in the topic to replace anything.");

                return;
            }

            uint pos;

            if (!uint.TryParse(command.Arguments.Groups["pos"].Value, out pos) || pos >= count)
            {
                command.Reply("There are only {0} topic parts. Remember indexes start at 0.", count);

                return;
            }

            topic[pos] = command.Arguments.Groups["text"].Value.Trim();

            var newTopic = string.Join(" | ", topic).Trim();

            if (newTopic == channel.Topic)
            {
                return;
            }

            Bootstrap.Client.Client.ChangeChannelTopic(command.Event.Recipient, newTopic);
        }
    }
}
