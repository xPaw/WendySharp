using System;
using System.Collections.Generic;
using NetIrc2;

namespace WendySharp
{
    class Voice : Command
    {
        public Voice()
        {
            Match = new List<string>
            {
                "voice",
                "hat",
            };
            Usage = "[nick] ...";
            ArgumentMatch = "(?<nicks>.+)?";
            HelpText = "Gives voice to the specified user.";
            Permission = "irc.op.voice";
        }

        public override void OnCommand(CommandArguments command)
        {
            var nicks = new List<IrcString>();
            var input = command.Arguments.Groups["nicks"].Value;

            if (input.Length == 0)
            {
                nicks.Add(command.Event.Sender.Nickname);
            }
            else
            {
                var nicksTemp = input.Split(' ');
                IrcIdentity ident;

                foreach (var nick in nicksTemp)
                {
                    if (!IrcIdentity.TryParse(nick, out ident))
                    {
                        command.Reply("'{0}' is a invalid indentity.", nick);

                        return;
                    }

                    nicks.Add(ident.Nickname);
                }
            }

            var channel = Bootstrap.Client.ChannelList.GetChannel(command.Event.Recipient);

            if (channel.WeAreOpped)
            {
                Bootstrap.Client.Client.Mode(command.Event.Recipient, "+" + new string('v', nicks.Count), nicks.ToArray());
            }
            else if (channel.HasChanServ)
            {
                Bootstrap.Client.Client.Message("ChanServ", string.Format("voice {0} {1}", channel.Name, string.Join(" ", nicks)));
            }
            else
            {
                command.Reply("I'm not opped, send help.");

                return;
            }

            Log.WriteInfo("Op", "{0} gave voice to {1} in {2}", command.Event.Sender, string.Join(", ", nicks), command.Event.Recipient);
        }
    }
}
