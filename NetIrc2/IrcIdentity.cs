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
using NetIrc2.Parsing;

namespace NetIrc2
{
    /// <summary>
    /// Stores an IRC user's identity information - their nickname, username, and hostname.
    /// </summary>
    public class IrcIdentity
    {
        /// <summary>
        /// Creates a new (blank) IRC identity.
        /// </summary>
        public IrcIdentity()
        {

        }

        /// <summary>
        /// Tries to parse a string to get an IRC identity.
        /// 
        /// IRC identities are formatted as nickname!username@hostname.
        /// </summary>
        /// <param name="string">The string to parse.</param>
        /// <param name="identity">The identity, or <c>null</c> if parsing fails.</param>
        /// <returns><c>true</c> if parsing completed successfully.</returns>
        public static bool TryParse(IrcString @string, out IrcIdentity identity)
        {
            IrcString[] parts; identity = null;
            if (@string == null) { goto invalid; }
            identity = new IrcIdentity();

            parts = @string.Split((byte)'@');
            if (parts.Length >= 2) { identity.Hostname = parts[1]; @string = parts[0]; }

            parts = @string.Split((byte)'!');
            if (parts.Length >= 2) { identity.Username = parts[1]; @string = parts[0]; }

            identity.Nickname = @string;

            string errorMessage;
            if (!IrcValidation.ValidateIdentity(identity, out errorMessage)) { goto invalid; }
            return true;

        invalid:
            identity = null; return false;
        }

        /// <summary>
        /// Converts an IRC identity into an IRC string.
        /// </summary>
        /// <returns>The IRC string.</returns>
        public IrcString ToIrcString()
        {
            string errorMessage;
            if (!IrcValidation.ValidateIdentity(this, out errorMessage)) { throw new InvalidOperationException(errorMessage); }

            var @string = Nickname;
            if (Username != null) { @string += "!" + Username; }
            if (Hostname != null) { @string += "@" + Hostname; }
            return @string;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            var id = obj as IrcIdentity;
            return id != null
                && object.Equals(Nickname, id.Nickname)
                && object.Equals(Username, id.Username)
                && object.Equals(Hostname, id.Hostname);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return (Nickname ?? IrcString.Empty).GetHashCode();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return ToIrcString();
        }

        /// <summary>
        /// Converts an IRC identity into an IRC string.
        /// </summary>
        /// <param name="identity">The IRC identity.</param>
        /// <returns>The IRC string.</returns>
        public static implicit operator IrcString(IrcIdentity identity)
        {
            return identity != null ? identity.ToIrcString() : null;
        }

        /// <summary>
        /// The user's IRC nickname (the name shown in channels).
        /// </summary>
        public IrcString Nickname
        {
            get;
            set;
        }

        /// <summary>
        /// The username.
        /// </summary>
        public IrcString Username
        {
            get;
            set;
        }

        /// <summary>
        /// The user's hostname.
        /// </summary>
        public IrcString Hostname
        {
            get;
            set;
        }

        /// <summary>
        /// Compares two identities for equality.
        /// </summary>
        /// <param name="identity1">The first identity.</param>
        /// <param name="identity2">The second identity.</param>
        /// <returns><c>true</c> if the identities are equal.</returns>
        public static bool operator ==(IrcIdentity identity1, IrcIdentity identity2)
        {
            return object.Equals(identity1, identity2);
        }

        /// <summary>
        /// Compares two identities for inequality.
        /// </summary>
        /// <param name="identity1">The first identity.</param>
        /// <param name="identity2">The second identity.</param>
        /// <returns><c>true</c> if the identities are not equal.</returns>
        public static bool operator !=(IrcIdentity identity1, IrcIdentity identity2)
        {
            return !object.Equals(identity1, identity2);
        }
    }
}
