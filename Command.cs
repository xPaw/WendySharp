using System.Text.RegularExpressions;

namespace WendySharp
{
    abstract class Command
    {
        /// <summary>
        /// Name of the command, used in command listing and usage text.
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// If specified, is a regular expression string specifying how
        /// to match just the command name (not the arguments). If it is not
        /// specified, the cmdname string is used to match the command. It should
        /// not have any capturing parenthesis since this will be concatenated with
        /// the argument matching regular expression.
        /// </summary>
        public string Match { get; protected set; }

        /// <summary>
        /// This is a regular expression string that matches the arguments of
        /// this command. The resulting match object is passed in to the callback.
        /// If not specified, the command takes no arguments. You probably want to
        /// put a $ at the end of this if given, otherwise any trailing string will
        /// still match.
        /// </summary>
        public string ArgumentMatch { get; protected set; }

        /// <summary>
        /// If specified, is the usage text for the command's arguments.
        /// For example, if the command takes two requried arguments and one optional
        /// argument, this should be set to something like "<arg1> <arg2> [arg3]"
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

            var pattern = string.Format("(?:{0}) (?:{1})", Match ?? Name, ArgumentMatch);

            Log.WriteDebug(Name, "Match with arguments: {0}", pattern);

            CompiledMatch = new Regex(pattern, RegexOptions.Compiled | RegexOptions.CultureInvariant);
        }
    }
}
