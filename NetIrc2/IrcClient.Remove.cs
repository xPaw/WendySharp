using NetIrc2.Details;

namespace NetIrc2
{
    partial class IrcClient
    {
        /// <summary>
        /// Removes the specified user from the channel. Channel operator access may be required.
        /// </summary>
        /// <param name="user">The user to remove.</param>
        /// <param name="channel">The channel to remove the user from.</param>
        /// <param name="reason">The reason the user was remove, or <c>null</c> to give no reason.</param>
        public void Remove(IrcString user, IrcString channel, IrcString reason)
        {
            Throw.If.Null(user, "user").Null(channel, "channel");

            IrcCommand("REMOVE", reason != null
                ? new IrcString[3] { channel, user, reason }
                : new IrcString[2] { channel, user });
        }
    }
}
