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

using System.Linq;

namespace NetIrc2.Parsing
{
    /// <summary>
    /// Validates various parameter types.
    /// </summary>
    public static class IrcValidation
    {
        /// <summary>
        /// Checks if the channel name is valid. The definition used by this test is somewhat loose.
        /// 
        /// Channel names may not contain spaces, commas, NULL, BELL, CR, or LF, and must start with # or &amp;.
        /// </summary>
        /// <param name="channel">The channel name to test.</param>
        /// <returns><c>true</c> if the name is valid.</returns>
        public static bool IsChannelName(IrcString channel)
        {
            if (channel == null) { return false; }

            string errorMessage;
            return ValidateChannel(channel, out errorMessage);
        }

        /// <summary>
        /// Checks if the nickname is valid. The definition used by this test is somewhat loose.
        /// 
        /// Nicknames may not contain spaces, commas, NULL, BELL, CR, LF, #, &amp;, @, or +.
        /// </summary>
        /// <param name="nickname">The nickname to test.</param>
        /// <returns><c>true</c> if the name is valid.</returns>
        public static bool IsNickname(IrcString nickname)
        {
            if (nickname == null) { return false; }

            string errorMessage;
            return ValidateNickname(nickname, out errorMessage);
        }

        internal static bool ValidateChannel(IrcString channel, out string errorMessage)
        {
            if (!ValidateTarget(channel, out errorMessage))
            {
                return false;
            }

            if (channel[0] != (byte)'#' && channel[0] != (byte)'&')
            {
                errorMessage = "Channel names must begin with # or &.";
                return false;
            }

            return true;
        }

        internal static bool ValidateNickname(IrcString nickname, out string errorMessage)
        {
            if (!ValidateTarget(nickname, out errorMessage))
            {
                return false;
            }

            if (nickname.Contains((byte)'#') ||
                nickname.Contains((byte)'&') ||
                nickname.Contains((byte)'@') ||
                nickname.Contains((byte)'+'))
            {
                errorMessage = "Nicknames may not contain #, &, @, or +.";
                return false;
            }

            return true;
        }

        internal static bool ValidateTarget(IrcString target, out string errorMessage)
        {
            if (!ValidateParameter(target, false, out errorMessage))
            {
                return false;
            }

            if (target.Length == 0)
            {
                errorMessage = "Targets may not be zero-byte.";
                return false;
            }

            if (target.Contains(7) || target.Contains((byte)','))
            {
                errorMessage = "Targets may not contain BELL or a comma.";
                return false;
            }

            return true;
        }

        internal static bool ValidateParameter(IrcString parameter, bool trailing, out string errorMessage)
        {
            errorMessage = null;

            if (!trailing && (parameter.Contains((byte)' ') || parameter.StartsWith(":")))
            {
                errorMessage = "Only the trailing parameter may contain spaces or start with a colon."; return false;
            }

            if (parameter.Contains(0) || parameter.Contains(13) || parameter.Contains(10))
            {
                errorMessage = "IRC does not allow embedded NULL, CR, or LF."; return false;
            }

            return true;
        }

        internal static bool ValidateIdentityPart(IrcString identityPart, out string errorMessage)
        {
            if (!ValidateParameter(identityPart, false, out errorMessage))
            {
                return false;
            }

            if (identityPart.Contains((byte)'@') || identityPart.Contains((byte)'!'))
            {
                errorMessage = "Identity parts may not contain @ or !."; return false;
            }

            return true;
        }

        internal static bool ValidateIdentity(IrcIdentity identity, out string errorMessage)
        {
            errorMessage = null;

            if (identity.Nickname == null || identity.Nickname.Length == 0)
            {
                errorMessage = "Nickname is not set."; return false;
            }

            if (!ValidateIdentityPart(identity.Nickname, out errorMessage))
            {
                return false;
            }

            if (identity.Username != null && !ValidateIdentityPart(identity.Username, out errorMessage))
            {
                return false;
            }

            if (identity.Hostname != null && !ValidateIdentityPart(identity.Hostname, out errorMessage))
            {
                return false;
            }

            return true;
        }

        internal static bool ValidateStatement(IrcStatement statement, out string errorMessage)
        {
            errorMessage = null;

            if (statement.Source != null && !ValidateIdentity(statement.Source, out errorMessage))
            {
                return false;
            }

            if (statement.Command == null)
            {
                errorMessage = "Command is not set."; return false;
            }

            if (statement.Command.Length == 3 && statement.Command.All(x => x >= (byte)'0' && x <= (byte)'9'))
            {
                // Command is a number.
            }
            else if (statement.Command.Length > 0 && statement.Command.All(x => x >= (byte)'A' && x <= (byte)'Z'))
            {
                // Command is letters.
            }
            else
            {
                errorMessage = "Command is invalid."; return false;
            }

            if (statement.Parameters.Count > IrcConstants.MaxParameters)
            {
                errorMessage = string.Format("IRC only allows up to {0} parameters.", IrcConstants.MaxParameters);
                return false;
            }

            for (int i = 0; i < statement.Parameters.Count; i++)
            {
                var parameter = statement.Parameters[i];
                if (parameter == null) { errorMessage = "No parameters may be null."; return false; }
                if (!ValidateParameter(parameter, i == statement.Parameters.Count - 1, out errorMessage)) { return false; }
            }

            return true;
        }
    }
}
