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

using System.Collections.Generic;
using System.Linq;

namespace NetIrc2
{
    partial class IrcClient
    {
        static IrcString CtcpEncode(IrcString command, IrcString[] parameters, bool escapeParameters)
        {
            return "\x1"
                + command
                + IrcString.Join("", parameters.Select(p => " " + (escapeParameters ? CtcpEscapeParameter(p) : p)).ToArray())
                + "\x1";
        }

        static IrcString CtcpEscapeParameter(IrcString parameter)
        {
            parameter = new IrcString(parameter.SelectMany(@byte =>
            {
                if (@byte == 0 || @byte == 1 || @byte == 13 || @byte == 10)
                {
                    return new byte[0];
                }

                if (@byte == (byte)'\\' || @byte == (byte)'\"')
                {
                    return new[] { (byte)'\\', @byte };
                }

                return new[] { @byte };
            }).ToArray());

            return parameter.Contains((byte)' ')
                ? "\"" + parameter + "\""
                : parameter;
        }

        static bool TryCtcpDecode(IrcString message, out IrcString command,
                                  out IrcString[] parameters, out IrcString rawParameter)
        {
            command = null; rawParameter = null; parameters = null;

            if (message.Length >= 2 && message[0] == 1 && message[message.Length - 1] == 1)
            {
                var args = message.Substring(1, message.Length - 2).Split((byte)' ', 2);
                command = args[0]; rawParameter = args.Length >= 2 ? args[1] : IrcString.Empty;

                var paramBytes = new List<List<byte>>();
                int index = 0; bool escaped = false, quoted = false;

                for (int i = 0; i < rawParameter.Length; i++)
                {
                    var @byte = rawParameter[i];
                    if (@byte == 0 || @byte == 1 || @byte == 13 || @byte == 10) { continue; }
                    byte? value = null;

                    if (escaped)
                    {
                        value = @byte; escaped = false;
                    }
                    else if (@byte == (byte)'\\')
                    {
                        escaped = true;
                    }
                    else if (@byte == (byte)'\"')
                    {
                        quoted = !quoted;
                    }
                    else if (@byte == (byte)' ')
                    {
                        if (quoted) { value = @byte; } else { index++; }
                    }
                    else
                    {
                        value = @byte;
                    }

                    if (value != null)
                    {
                        while (paramBytes.Count <= index) { paramBytes.Add(new List<byte>()); }
                        paramBytes[index].Add((byte)value);
                    }
                }

                parameters = paramBytes.Select(paramByte => new IrcString(paramByte.ToArray())).ToArray();
                return true;
            }
            
            return false;
        }
    }
}
