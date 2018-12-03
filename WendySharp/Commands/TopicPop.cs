using System;
using System.Collections.Generic;
using System.Linq;

namespace WendySharp
{
    class TopicPop : Command
    {
        public TopicPop()
        {
            Match = new List<string>
            {
                "topicpop",
                "tpop",
            };
            HelpText = "Removes the last topic item.";
            Permission = "irc.op.topic";
        }

        public override void OnCommand(CommandArguments command)
        {
            var channel = Bootstrap.Client.ChannelList.GetChannel(command.Event.Recipient);

            var topic = channel.Topic.Split(new [] { " | " }, StringSplitOptions.None);

            if (topic.Length < 2)
            {
                command.Reply("Not enough parts in the topic to pop anything.");

                return;
            }

            var newTopic = string.Join(" | ", topic.Take(topic.Length - 1)).Trim();

            if (newTopic == channel.Topic)
            {
                return;
            }

            Bootstrap.Client.Client.ChangeChannelTopic(command.Event.Recipient, newTopic);
        }
    }
}
