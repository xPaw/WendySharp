using System;
using NetIrc2;

namespace WendySharp
{
    class Flex : Command
    {
        public Flex()
        {
            Name = "flex";
            HelpText = "OPs you for a few seconds, to show off your powah!";
            Permission = "irc.op.flex";
        }

        public override void OnCommand(CommandArguments command)
        {
            // TODO: If we're not op, we should try to gain op
            var channel = Bootstrap.Client.ChannelList.GetChannel(command.Event.Recipient);

            if (!channel.WeAreOpped)
            {
                command.Reply("I'm not opped, send help.");

                return;
            }

            if (channel.Users[command.Event.Sender.Nickname] == Channel.Operator)
            {
                command.Reply("Silly billy, you're already an operator.");

                return;
            }

            Bootstrap.Client.Client.Mode(command.Event.Recipient, "+o", new IrcString[1] { command.Event.Sender.Nickname });

            Bootstrap.Client.ModeList.AddLateModeRequest(
                new LateModeRequest
                {
                    Channel = command.Event.Recipient,
                    Recipient = command.Event.Sender.Nickname,
                    Mode = "-o",
                    Time = DateTime.UtcNow.AddSeconds(10),
                    Reason = "Used flex command"
                }
            );
        }
    }
}
