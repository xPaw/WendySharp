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
    /// Stores an entry of the channel list.
    /// </summary>
    public class ChannelListEntryEventArgs : EventArgs
    {
        /// <summary>
        /// Creates a new instance of <see cref="ChannelListEntryEventArgs"/>.
        /// </summary>
        /// <param name="channel">The channel name.</param>
        /// <param name="userCount">The number of users in the channel.</param>
        /// <param name="topic">The channel topic.</param>
        public ChannelListEntryEventArgs(IrcString channel, int userCount, IrcString topic)
        {
            Throw.If.Null(channel, "channel").Negative(userCount, "userCount").Null(topic, "topic");

            Channel = channel; UserCount = userCount; Topic = topic;
        }

        /// <summary>
        /// The channel name.
        /// </summary>
        public IrcString Channel
        {
            get;
            private set;
        }

        /// <summary>
        /// The number of users in the channel.
        /// </summary>
        public int UserCount
        {
            get;
            private set;
        }

        /// <summary>
        /// The channel topic.
        /// </summary>
        public IrcString Topic
        {
            get;
            private set;
        }
    }
}
