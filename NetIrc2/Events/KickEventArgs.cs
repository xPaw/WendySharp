#region License
/*
NetIRC2
Copyright (c) 2013 James F. Bellinger <http://www.zer7.com>
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met: 

1. Redistributions of source code must retain the above copyright notice, this
   list of conditions and the following disclaimer. 
2. Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution. 

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
#endregion

using NetIrc2.Details;

namespace NetIrc2.Events
{
    /// <summary>
    /// Stores information about a user being kicked from a channel.
    /// </summary>
    public class KickEventArgs : TargetedMessageEventArgs
    {
        /// <summary>
        /// Creates a new instance of <see cref="KickEventArgs"/>.
        /// </summary>
        /// <param name="sender">The user doing the kicking.</param>
        /// <param name="recipient">The user being kicked out of the channel.</param>
        /// <param name="channel">The channel the user is being kicked from.</param>
        /// <param name="reason">The reason the user is being kicked, or <c>null</c> if none is given.</param>
        public KickEventArgs(IrcIdentity sender, IrcString recipient, IrcString channel, IrcString reason)
            : base(sender, recipient)
        {
            Throw.If.Null(channel, "channel");

            Channel = channel; Reason = reason;
        }

        /// <summary>
        /// The channel the user is being kicked from.
        /// </summary>
        public IrcString Channel
        {
            get;
            private set;
        }

        /// <summary>
        /// The reason the user is being kicked, or <c>null</c> if none is given.
        /// </summary>
        public IrcString Reason
        {
            get;
            private set;
        }
    }
}
