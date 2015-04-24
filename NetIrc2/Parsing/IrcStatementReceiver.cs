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
using System.Diagnostics;
using System.IO;
using NetIrc2.Details;

namespace NetIrc2.Parsing   
{
    /// <summary>
    /// Receives IRC statements from a stream.
    /// </summary>
    public sealed class IrcStatementReceiver
    {
        byte[] _buffer = new byte[IrcConstants.MaxStatementLength];
        int _count = 0;
        Stream _stream;

        /// <summary>
        /// Creates a new receiver.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        public IrcStatementReceiver(Stream stream)
        {
            Throw.If.Null(stream, "stream");
            _stream = stream;
        }

        /// <summary>
        /// Tries to receive an IRC statement.
        /// 
        /// A blocking read is used.
        /// If you have a timeout set, <paramref name="parseResult"/> may be <see cref="IrcStatementParseResult.TimedOut"/>.
        /// </summary>
        /// <param name="statement">The statement.</param>
        /// <param name="parseResult">The parse result.</param>
        /// <returns><c>true</c> if a complete IRC statement was received.</returns>
        public bool TryReceive(out IrcStatement statement, out IrcStatementParseResult parseResult)
        {
            if (TryReceiveFromBuffer(out statement, out parseResult))
            {
                return true;
            }

            var receiveResult = TryReceiveFromStream();
            if (receiveResult != IrcStatementParseResult.OK)
            {
                statement = null; parseResult = receiveResult;
                return false;
            }

            return TryReceiveFromBuffer(out statement, out parseResult);
        }

        bool TryReceiveFromBuffer(out IrcStatement statement, out IrcStatementParseResult parseResult)
        {
            int offset = 0;
            bool gotStatement = IrcStatement.TryParse(_buffer, ref offset, _count, out statement, out parseResult);
            Debug.Assert(offset <= _count);

            if (offset != 0) { Array.Copy(_buffer, offset, _buffer, 0, _count - offset); _count -= offset; }
            return gotStatement;
        }

        IrcStatementParseResult TryReceiveFromStream()
        {
            int thisCount;
            try { thisCount = _stream.Read(_buffer, _count, _buffer.Length - _count); }
            catch (IOException) { thisCount = 0; }
            catch (ObjectDisposedException) { thisCount = 0; }
            catch (TimeoutException) { return IrcStatementParseResult.TimedOut; }

            if (thisCount == 0) { return IrcStatementParseResult.Disconnected; }
            _count += thisCount; return IrcStatementParseResult.OK;
        }
    }
}
