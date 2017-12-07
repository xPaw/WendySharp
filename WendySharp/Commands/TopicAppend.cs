using System.Collections.Generic;

namespace WendySharp
{
    class TopicAppend : Command
    {
        public TopicAppend()
        {
            Match = new List<string>
            {
                "topicappend",
                "tappend",
                "tadd",
            };
            Usage = "<text>";
            ArgumentMatch = "(?<text>.+)$";
            HelpText = "Appends text to the end of the channel topic.";
            Permission = "irc.op.topic";
        }

        public override void OnCommand(CommandArguments command)
        {
            var channel = Bootstrap.Client.ChannelList.GetChannel(command.Event.Recipient);
            var newTopic = string.Format("{0} | {1}", channel.Topic.Trim(), command.Arguments.Groups["text"].Value.Trim());

            if (newTopic == channel.Topic)
            {
                return;
            }

            Bootstrap.Client.Client.ChangeChannelTopic(command.Event.Recipient, newTopic);
        }
    }
}
