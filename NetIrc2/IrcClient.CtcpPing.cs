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
using System.Security.Cryptography;

namespace NetIrc2
{
    partial class IrcClient
    {
        // This slightly convoluted approach to CTCP Ping means we don't reveal anything
        // about how long the system's been running (Environment.TickCount). That information
        // might be useful if the same PC is running other services that use a RNG seeded by it.
        const int _ctcpMask = 0xffff;
        ushort _ctcpPingOffset;

        void CtcpPingInit()
        {
            var offsetBytes = new byte[2];
            new RNGCryptoServiceProvider().GetBytes(offsetBytes);
            _ctcpPingOffset = BitConverter.ToUInt16(offsetBytes, 0);
        }

        int CtcpPingGetTimestamp()
        {
            return (Environment.TickCount + _ctcpPingOffset) & _ctcpMask;
        }

        int CtcpPingGetTimeDifference(int start, int current)
        {
            return (current - start) & _ctcpMask;
        }
    }
}
