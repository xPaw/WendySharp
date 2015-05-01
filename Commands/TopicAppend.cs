namespace WendySharp
{
    class TopicAppend : Command
    {
        public TopicAppend()
        {
            Name = "topicappend";
            Match = "topicappend|tappend|tadd";
            Usage = "<text>";
            ArgumentMatch = "(?<text>.+)$";
            HelpText = "Appends text to the end of the channel topic.";
            Permission = "irc.op.topic";
        }

        public override void OnCommand(CommandArguments command)
        {
            var channel = Bootstrap.Client.ChannelList.GetChannel(command.Event.Recipient);

            Bootstrap.Client.Client.ChangeChannelTopic(command.Event.Recipient, string.Format("{0} | {1}", channel.Topic.Trim(), command.Arguments.Groups["text"].Value.Trim()));
        }
    }
}
