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

using System;
using NetIrc2.Details;

namespace NetIrc2.Events
{
    /// <summary>
    /// Stores information about a user disconnecting from an IRC server.
    /// </summary>
    public class QuitEventArgs : EventArgs
    {
        /// <summary>
        /// Creates a new instance of <see cref="QuitEventArgs"/>.
        /// </summary>
        /// <param name="identity">The user who disconnected.</param>
        /// <param name="quitMessage">The quit message, or <c>null</c> if none was given.</param>
        public QuitEventArgs(IrcIdentity identity, IrcString quitMessage)
        {
            Throw.If.Null(identity, "identity");

            Identity = identity; QuitMessage = quitMessage;
        }

        /// <summary>
        /// The user who disconnected.
        /// </summary>
        public IrcIdentity Identity
        {
            get;
            private set;
        }

        /// <summary>
        /// The quit message, or <c>null</c> if none was given.
        /// </summary>
        public IrcString QuitMessage
        {
            get;
            private set;
        }
    }
}
