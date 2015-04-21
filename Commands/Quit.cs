using System;

namespace WendySharp
{
    class Quit : Command
    {
        public Quit()
        {
            Name = "shutdown";
            Match = "kill|die|stop|quit|shutdown";
            HelpText = "Shuts down";
            Permission = "core.shutdown";
        }

        public override void OnCommand(CommandArguments command)
        {
            command.Reply("Farewell");

            Bootstrap.ResetEvent.Set();
        }
    }
}
