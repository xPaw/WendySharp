using System;
using System.Collections.Generic;

namespace WendySharp
{
    class Topic : Command
    {
        public Topic()
        {
            Match = new List<string>
            {
                "topic"
            };
            Usage = "<text>";
            ArgumentMatch = "(?<text>.+)$";
            HelpText = "Set a topic in a channel.";
            Permission = "irc.op.topic";
        }

        public override void OnCommand(CommandArguments command)
        {
            var channel = Bootstrap.Client.ChannelList.GetChannel(command.Event.Recipient);
            var newTopic = command.Arguments.Groups["text"].Value.Trim();

            if (newTopic == channel.Topic)
            {
                return;
            }

            Bootstrap.Client.Client.ChangeChannelTopic(command.Event.Recipient, newTopic);
        }
    }
}
