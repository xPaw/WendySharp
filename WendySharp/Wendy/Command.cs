using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace WendySharp
{
    abstract class Command
    {
        /// <summary>
        /// Command triggers.
        /// First entry is used in command listing and usage text.
        /// </summary>
        public List<string> Match { get; protected set; }

        /// <summary>
        /// This is a regular expression string that matches the arguments of
        /// this command. The resulting match object is passed in to the callback.
        /// If not specified, the command takes no arguments. You probably want to
        /// put a $ at the end of this if given, otherwise any trailing string will
        /// still match.
        /// </summary>
        protected string ArgumentMatch { private get; set; }

        /// <summary>
        /// If specified, is the usage text for the command's arguments.
        /// For example, if the command takes two requried arguments and one optional
        /// argument, this should be set to something like "&lt;arg1> &lt;arg2> [arg3]"
        /// </summary>
        public string Usage { get; protected set; }

        /// <summary>
        /// If given, is displayed after the usage in help messages as a
        /// short one-line description of this command.
        /// </summary>
        public string HelpText { get; protected set; }

        /// <summary>
        /// If given, is the permission required for a user to execute this command.
        /// </summary>
        public string Permission { get; protected set; }

        public Regex CompiledMatch { get; private set; }

        /// <summary>
        /// Executed when an user calls this command.
        /// </summary>
        public abstract void OnCommand(CommandArguments command);

        public void Compile()
        {
            if (ArgumentMatch == null)
            {
                return;
            }

            // Explanation of the (?: |\b):
            // normally we want to match a space between the command and its
            // arguments, but to support possibly empty arguments or fancier
            // regular expressions, we also accept the empty string between the
            // command and its arguments as long as it's a word boundary. This
            // probably strictly isn't the right thing to do, but the only thing
            // this really disallows is having no boundry between the command
            // and its arguments.
            var pattern = string.Format(@"({0})( |\b){1}", string.Join("|", Match), ArgumentMatch);

            CompiledMatch = new Regex(pattern, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);
        }
    }
}
