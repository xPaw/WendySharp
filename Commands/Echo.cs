using System;

namespace WendySharp
{
    class Echo : Command
    {
        public Echo()
        {
            Name = "echo";
            Usage = "<text to echo>";
            ArgumentMatch = "(?<message>.+)$";
            HelpText = "Echos text back to where it came from";
            Permission = "irc.echo";
        }

        public override void OnCommand(CommandArguments command)
        {
            var message = command.Arguments.Groups["message"].Value;

            Log.WriteInfo("Echo", "{0} echo-ed in {1}: '{2}'", command.Event.Sender, command.Event.Recipient, message);

            Bootstrap.Client.Client.Message(command.Event.Recipient, message);
        }
    }
}
