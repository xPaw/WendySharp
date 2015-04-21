using System.Text.RegularExpressions;

namespace WendySharp
{
    abstract class Command
    {
        public string Name { get; protected set; }
        public string Match { get; protected set; }
        public string ArgumentMatch { get; protected set; }
        public string Usage { get; protected set; }
        public string HelpText { get; protected set; }
        public string Permission { get; protected set; }

        public Regex CompiledMatch { get; private set; }

        public abstract void OnCommand(CommandArguments command);

        public void Compile()
        {
            if (ArgumentMatch == null)
            {
                return;
            }

            var pattern = string.Format("(?:{0}) (?:{1})", Match ?? Name, ArgumentMatch);

            Log.WriteDebug(Name, "Match with arguments: {0}", pattern);

            CompiledMatch = new Regex(pattern, RegexOptions.Compiled | RegexOptions.CultureInvariant);
        }
    }
}
