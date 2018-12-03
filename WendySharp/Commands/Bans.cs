using System;
using System.Collections.Generic;
using System.Linq;

namespace WendySharp
{
    class Bans : Command
    {
        public Bans()
        {
            Match = new List<string>
            {
                "bans"
            };
            Usage = "<mask> | all";
            ArgumentMatch = "(?<mask>[^ ])?";
            HelpText = "Lists the time until given mask is unbanned.";
            Permission = "irc.op.bans";
        }

        public override void OnCommand(CommandArguments command)
        {
            // TODO: implement mask search
            //var mask = command.Arguments.Groups["mask"].Value;

            var modes = Bootstrap.Client.ModeList.GetModes().Where(x => x.Time != default(DateTime)).OrderBy(x => x.Time).ToList();

            if (modes.Count == 0)
            {
                command.ReplyAsNotice("No pending unbans in this channel.");

                return;
            }

            foreach (var mode in modes)
            {
                command.ReplyAsNotice("In {0} setting {1} {2} {3}{4}",
                    mode.Channel,
                    mode.Mode,
                    mode.Recipient,
                    mode.Time.ToRelativeString(),
                    mode.Sender == null ? "" : string.Format(" (from {0})", mode.Sender)
                );
            }
        }
    }
}
