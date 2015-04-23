using System;
using System.Linq;

namespace WendySharp
{
    class Bans : Command
    {
        public Bans()
        {
            Name = "bans";
            Usage = "<mask> | all";
            ArgumentMatch = "(?<mask>[^ ])?";
            HelpText = "Lists the time until given mask is unbanned";
            Permission = "irc.op.bans";
        }

        public override void OnCommand(CommandArguments command)
        {
            //var mask = command.Arguments.Groups["mask"].Value;

            var modes = Bootstrap.Client.ModeList.GetModes().OrderBy(x => x.Time);

            foreach (var mode in modes)
            {
                command.Reply("In channel {0} setting {1} {2} {3}", mode.Channel, mode.Mode, mode.Recipient, mode.Time.ToRelativeString());
            }
        }
    }
}
