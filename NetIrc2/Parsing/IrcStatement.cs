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
using System.Collections.Generic;
using System.Linq;
using NetIrc2.Details;

namespace NetIrc2.Parsing
{
    /// <summary>
    /// Reads and writes raw IRC statement lines.
    /// </summary>
    public class IrcStatement
    {
        /// <summary>
        /// Creates an IRC statement with nothing set.
        /// </summary>
        public IrcStatement()
        {
            Parameters = new List<IrcString>();
        }

        /// <summary>
        /// Creates an IRC statement.
        /// </summary>
        /// <param name="source">The source of the statement, if any. This is called the prefix in the IRC specification.</param>
        /// <param name="command">The command or three-digit reply code.</param>
        /// <param name="parameters">The parameters of the command.</param>
        public IrcStatement(IrcIdentity source, IrcString command, params IrcString[] parameters)
        {
            Source = source; Command = command; Parameters = new List<IrcString>(parameters);
        }

        static void SkipCrlf(byte[] buffer, ref int offset, ref int count)
        {
            while (count > 0 && (buffer[offset] == 13 || buffer[offset] == 10))
            {
                offset++; count--;
            }
        }

        /// <summary>
        /// Tries to read a buffer and parse out an IRC statement. 
        /// </summary>
        /// <param name="buffer">The buffer to read from.</param>
        /// <param name="offset">The offset to begin reading. The parser may advance this, even if parsing fails.</param>
        /// <param name="count">The maximum number of bytes to read.</param>
        /// <param name="statement">The statement, if parsing succeeds, or <c>null</c>.</param>
        /// <returns><c>true</c> if parsing succeeded.</returns>
        public static bool TryParse(byte[] buffer, ref int offset, int count,
                                    out IrcStatement statement)
        {
            IrcStatementParseResult parseResult;
            return TryParse(buffer, ref offset, count, out statement, out parseResult);
        }

        /// <summary>
        /// Tries to read a buffer and parse out an IRC statement. 
        /// Additionally, on failure, the reason for failure is returned.
        /// </summary>
        /// <param name="buffer">The buffer to read from.</param>
        /// <param name="offset">The offset to begin reading. The parser may advance this, even if parsing fails.</param>
        /// <param name="count">The maximum number of bytes to read.</param>
        /// <param name="statement">The statement, if parsing succeeds, or <c>null</c>.</param>
        /// <param name="parseResult">The result of parsing. On failure, this is the reason for the failure.</param>
        /// <returns><c>true</c> if parsing succeeded.</returns>
        public static bool TryParse(byte[] buffer, ref int offset, int count,
                                    out IrcStatement statement, out IrcStatementParseResult parseResult)
        {
            Throw.If.Null(buffer, "buffer");
            string errorMessage = null;

            IrcString[] parts;
            statement = null; parseResult = IrcStatementParseResult.NothingToParse;

            // First, skip all initial CR/LF.
            SkipCrlf(buffer, ref offset, ref count);

            // See if we've got a CR or LF anywhere.
            int crlfIndex = IrcString.IndexOf(buffer, @byte => @byte == 13 || @byte == 10, offset, count);
            if (crlfIndex == -1)
            {
                if (count >= IrcConstants.MaxStatementLength)
                {
                    parseResult = IrcStatementParseResult.StatementTooLong;
                }

                return false;
            }

            // OK, let's get our string.
            var @string = new IrcString(buffer, offset, crlfIndex - offset);

#if DEBUG
            var debugString = @string;
#endif
            offset += @string.Length + 1; count -= @string.Length + 1;
            SkipCrlf(buffer, ref offset, ref count);

            if (crlfIndex >= IrcConstants.MaxStatementLength)
            {
                parseResult = IrcStatementParseResult.StatementTooLong; return false;
            }

            // Do we have a prefix?
            statement = new IrcStatement();

            if (@string.Length >= 1 && @string[0] == (byte)':')
            {
                parts = @string.Split((byte)' ', 2);

                var sourceString = parts[0].Substring(1); IrcIdentity source;
                if (!IrcIdentity.TryParse(sourceString, out source)) { goto invalid; }
                statement.Source = source;

                @string = parts.Length >= 2 ? parts[1] : IrcString.Empty;
            }

            // Now get the command.
            parts = @string.Split((byte)' ', 2);
            statement.Command = parts[0];
            @string = parts.Length >= 2 ? parts[1] : IrcString.Empty;

            // Parameters, now...
            while (@string.Length > 0)
            {
                if (@string[0] == (byte)':')
                {
                    statement.Parameters.Add(@string.Substring(1));
                    break;
                }
                else
                {
                    parts = @string.Split((byte)' ', 2);
                    statement.Parameters.Add(parts[0]);
                    if (parts.Length == 1) { break; }
                    @string = parts[1];
                }
            }

            // We're done. If everything's kosher, we'll return true.
            if (!IrcValidation.ValidateStatement(statement, out errorMessage))
            {
                goto invalid;
            }

            parseResult = IrcStatementParseResult.OK;
            return true;

        invalid:
#if DEBUG
            Console.WriteLine("Invalid statement '{0}' (error '{1}').", debugString, errorMessage);
#endif
            statement = null; parseResult = IrcStatementParseResult.InvalidStatement;
            return false;
        }

        /// <summary>
        /// Converts the IRC statement into a byte array, including the ending CR+LF.
        /// </summary>
        /// <returns>A byte array.</returns>
        public byte[] ToByteArray()
        {
            return ToIrcString();
        }

        /// <summary>
        /// Converts the IRC statement into a byte array, including the ending CR+LF,
        /// and additionally returns whether the string was truncated.
        /// </summary>
        /// <param name="truncated"><c>true</c> if the string was too long and had to be truncated.</param>
        /// <returns>A byte array.</returns>
        public byte[] ToByteArray(out bool truncated)
        {
            return ToIrcString(out truncated);
        }

        /// <summary>
        /// Converts the IRC statement into an IRC string containing all of its bytes,
        /// including the ending CR+LF.
        /// </summary>
        /// <returns>An IRC string.</returns>
        public IrcString ToIrcString()
        {
            bool truncated;
            return ToIrcString(out truncated);
        }

        /// <summary>
        /// Converts the IRC statement into an IRC string containing all of its bytes,
        /// including the ending CR+LF, and additionally returns whether the string was truncated.
        /// </summary>
        /// <param name="truncated"><c>true</c> if the string was too long and had to be truncated.</param>
        /// <returns>An IRC string.</returns>
        public IrcString ToIrcString(out bool truncated)
        {
            string errorMessage;
            if (!IrcValidation.ValidateStatement(this, out errorMessage))
            {
                throw new InvalidOperationException(errorMessage);
            }

            // Convert to an IrcString first.
            truncated = false; var @string = IrcString.Empty;

            if (Source != null)
            {
                @string += ":" + Source.ToIrcString() + " ";
            }

            @string += Command;

            for (int i = 0; i < Parameters.Count; i++)
            {
                var parameter = Parameters[i];

                @string += " ";
                if (i == Parameters.Count - 1 && parameter.Contains((byte)' ')) { @string += ":"; }
                @string += parameter;
            }

            // Now convert to a byte array, truncate if need be, and add CR+LF.
            if (@string.Length > IrcConstants.MaxStatementLength - 2)
            {
                @string = @string.Substring(0, IrcConstants.MaxStatementLength - 2);
                truncated = true;
            }

            @string += "\r\n";
            return @string;
        }

        /// <summary>
        /// The source of the statement, if any. This is called the prefix in the IRC specification.
        /// </summary>
        public IrcIdentity Source
        {
            get;
            set;
        }

        /// <summary>
        /// The command, or if the IRC statement is a reply, a three-digit number.
        /// </summary>
        public IrcString Command
        {
            get;
            set;
        }

        /// <summary>
        /// The numeric reply code, if the IRC statement is a reply.
        /// </summary>
        public IrcReplyCode ReplyCode
        {
            get { int value; return (IrcReplyCode)(int.TryParse(Command, out value) ? value : 0); }
            set { Command = ((int)value).ToString(); }
        }

        /// <summary>
        /// The parameters of the statement.
        /// </summary>
        public IList<IrcString> Parameters
        {
            get;
            private set;
        }
    }
}
