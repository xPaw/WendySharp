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
    /// Stores a sender and recipient for a targeted action.
    /// </summary>
    public class TargetedMessageEventArgs : EventArgs
    {
        /// <summary>
        /// Creates a new instance of <see cref="TargetedMessageEventArgs"/>.
        /// </summary>
        /// <param name="sender">The sender, or <c>null</c> if the message has no sender.</param>
        /// <param name="recipient">The recipient.</param>
        public TargetedMessageEventArgs(IrcIdentity sender, IrcString recipient)
        {
            Throw.If.Null(recipient, "recipient");

            Sender = sender; Recipient = recipient;
        }

        /// <summary>
        /// The sender.
        /// 
        /// Be aware that some messages may not have a sender, such as NOTICEs from
        /// the server at connect time. In this case the sender will be <c>null</c>.
        /// </summary>
        public IrcIdentity Sender
        {
            get;
            private set;
        }

        /// <summary>
        /// The recipient.
        /// </summary>
        public IrcString Recipient
        {
            get;
            private set;
        }
    }
}
