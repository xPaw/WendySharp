using System.Collections.Generic;

namespace WendySharp
{
    class Quit : Command
    {
        public Quit()
        {
            Match = new List<string>
            {
                "shutdown",
                "stop",
                "quit",
            };
            HelpText = "Shuts down.";
            Permission = "core.shutdown";
        }

        public override void OnCommand(CommandArguments command)
        {
            command.Reply("Farewell");

            Bootstrap.ResetEvent.Set();
        }
    }
}
