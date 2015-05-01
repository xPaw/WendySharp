using System;
using System.Collections.Generic;
using NetIrc2;

namespace WendySharp
{
    class Devoice : Command
    {
        public Devoice()
        {
            Match = new List<string>
            {
                "devoice",
                "dehat",
                "unhat"
            };
            Usage = "[nick] ...";
            ArgumentMatch = "(?<nicks>.+)?";
            HelpText = "Takes voice from the specified user.";
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
                Bootstrap.Client.Client.Mode(command.Event.Recipient, "-" + new string('o', nicks.Count), nicks.ToArray());
            }
            else if (channel.HasUser("ChanServ"))
            {
                Bootstrap.Client.Client.Message("ChanServ", string.Format("devoice {0} {1}", channel.Name, string.Join(" ", nicks)));
            }
            else
            {
                command.Reply("I'm not opped, send help.");

                return;
            }

            Log.WriteInfo("Deop", "{0} took voice from {1} in {2}", command.Event.Sender, string.Join(", ", nicks), command.Event.Recipient);
        }
    }
}
