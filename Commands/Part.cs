using System;

namespace WendySharp
{
    class Part : Command
    {
        public Part()
        {
            Name = "part";
            Match = "part|leave";
            Usage = "[channel]";
            ArgumentMatch = "(?<channel>#+[\\w-]+)?$";
            HelpText = "Leaves the current or specified IRC channel";
            Permission = "irc.control";
        }

        public override void OnCommand(CommandArguments command)
        {
            var channel = command.Arguments.Groups["channel"].Value;

            if (string.IsNullOrEmpty(channel))
            {
                if (Bootstrap.IsRecipientChannel(command.Event.Recipient))
                {
                    channel = command.Event.Recipient;

                    Bootstrap.Client.Client.Message(channel, string.Format("Farewell, {0}!", channel));
                }
                else
                {
                    command.Reply("Tell me which channel to leave.");

                    return;
                }
            }

            Log.WriteInfo("Part", "{0} made us leave channel {1}", command.Event.Sender, channel);

            Bootstrap.Client.Client.Leave(channel);

            command.Reply("Left {0}", channel);
        }
    }
}
