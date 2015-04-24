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
    /// Stores a mode change.
    /// </summary>
    public class ModeEventArgs : TargetedMessageEventArgs
    {
        IrcString[] _parameters;

        /// <summary>
        /// Creates a new instance of <see cref="ModeEventArgs"/>.
        /// </summary>
        /// <param name="sender">The user changing the mode.</param>
        /// <param name="recipient">The target of the mode change. This may be a channel or a user.</param>
        /// <param name="command">The mode change, for example +o or +v.</param>
        /// <param name="parameters">The mode change parameters.</param>
        public ModeEventArgs(IrcIdentity sender, IrcString recipient, IrcString command, IrcString[] parameters)
            : base(sender, recipient)
        {
            Throw.If.Null(command, "command").NullElements(parameters, "parameters");

            Command = command; _parameters = (IrcString[])parameters.Clone();
        }

        /// <summary>
        /// Gets a mode change parameter.
        /// </summary>
        /// <param name="index">The index of the parameter.</param>
        /// <returns>A parameter.</returns>
        public IrcString GetParameter(int index)
        {
            return _parameters[index];
        }

        /// <summary>
        /// Gets all of the mode change parameters.
        /// </summary>
        /// <returns>An array of parameters.</returns>
        public IrcString[] GetParameterList()
        {
            return (IrcString[])_parameters.Clone();
        }

        /// <summary>
        /// The mode change, for example +o or +v.
        /// </summary>
        public IrcString Command
        {
            get;
            private set;
        }

        /// <summary>
        /// The number of parameters.
        /// </summary>
        public int ParameterCount
        {
            get { return _parameters.Length; }
        }
    }
}
