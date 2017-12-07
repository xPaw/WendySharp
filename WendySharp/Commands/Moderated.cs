using System;
using System.Collections.Generic;
using NetIrc2;

namespace WendySharp
{
    class Moderated : Command
    {
        public Moderated()
        {
            Match = new List<string>
            {
                "m",
            };
            HelpText = "FOR EMERGENCY USE ONLY! Sets +m on the channel to quiet it in an emergency";
            Permission = "irc.op.m";
        }

        public override void OnCommand(CommandArguments command)
        {
            var channel = Bootstrap.Client.ChannelList.GetChannel(command.Event.Recipient);

            // TODO: Check current channel modes

            if (channel.WeAreOpped)
            {
                Bootstrap.Client.Client.Mode(command.Event.Recipient, "+o", command.Event.Sender.Nickname);
            }
            else if (channel.HasChanServ)
            {
                // Op both the bot and sender
                Bootstrap.Client.Client.Message("ChanServ", string.Format("op {0} {1} {2}", channel.Name, Bootstrap.Client.TrueNickname, command.Event.Sender.Nickname));
            }
            else
            {
                command.Reply("I'm not opped, send help.");

                return;
            }

            Log.WriteInfo("Moderated", "'{0}' made the '{1}' go into emergency mode", command.Event.Sender, command.Event.Recipient);

            Bootstrap.Client.Client.Mode(command.Event.Recipient, "+m");

            command.ReplyAsNotice("Channel is now in LOCKDOWN mode. Only operators and voiced users may speak.");
            //command.ReplyAsNotice("Use {0}m again to undo and revert to normal", Bootstrap.Client.Settings.Prefix);
        }
    }
}
