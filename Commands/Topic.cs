namespace WendySharp
{
    class Topic : Command
    {
        public Topic()
        {
            Name = "topic";
            Usage = "<text>";
            ArgumentMatch = "(?<text>.+)$";
            HelpText = "Set a topic in a channel.";
            Permission = "irc.op.topic";
        }

        public override void OnCommand(CommandArguments command)
        {
            Bootstrap.Client.Client.ChangeChannelTopic(command.Event.Recipient, command.Arguments.Groups["text"].Value.Trim());
        }
    }
}
