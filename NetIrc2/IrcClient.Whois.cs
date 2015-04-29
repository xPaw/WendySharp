using NetIrc2.Details;

namespace NetIrc2
{
    partial class IrcClient
    {
        /// <summary>
        /// This command is used to query information about particular user.
        /// </summary>
        /// <param name="recipient">The user to query.</param>
        public void Whois(IrcString recipient)
        {
            Throw.If.Null(recipient, "recipient");

            IrcCommand("WHOIS", recipient);
        }
    }
}
