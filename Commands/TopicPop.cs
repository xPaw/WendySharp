using System;
using System.Linq;

namespace WendySharp
{
    class TopicPop : Command
    {
        public TopicPop()
        {
            Name = "topicpop";
            Match = "topicpop|tpop";
            HelpText = "Removes the last topic item.";
            Permission = "irc.op.topic";
        }

        public override void OnCommand(CommandArguments command)
        {
            var channel = Bootstrap.Client.ChannelList.GetChannel(command.Event.Recipient);

            var topic = channel.Topic.Split(new [] { " | " }, StringSplitOptions.None);
            var count = topic.Count();

            if (count < 2)
            {
                command.Reply("Not enough parts in the topic to pop anything.");

                return;
            }

            Bootstrap.Client.Client.ChangeChannelTopic(command.Event.Recipient, string.Join(" | ", topic.Take(count - 1)).Trim());
        }
    }
}
