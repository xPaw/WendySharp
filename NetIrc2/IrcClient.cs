#region License
/*
NetIRC2
Copyright (c) 2005, 2013 James F. Bellinger <http://www.zer7.com>
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
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using NetIrc2.Details;
using NetIrc2.Parsing;

namespace NetIrc2
{
    /// <summary>
    /// Communicates with an Internet Relay Chat server.
    /// </summary>
    [Category("Network")]
    [Description("Communicates with an Internet Relay Chat server.")]
    public partial class IrcClient
    {
        class Context
        {
            public Thread ReceiverThread;
            public ManualResetEvent StartEvent;
            public Stream Stream;
            public SynchronizationContext SynchronizationContext;
        }
        Context _context;
        IrcString _clientVer;

        static IrcClient()
        {
            CtcpTimeInit();
        }

        /// <summary>
        /// Creates a new IRC client.
        /// </summary>
        public IrcClient()
        {
            CtcpPingInit();
            SyncRoot = new object();
        }

        /// <summary>
        /// Connects to an IRC server.
        /// </summary>
        /// <param name="hostname">The server hostname.</param>
        /// <param name="port">The server port.</param>
        /// <param name="options">Options for the connection, if any, or <c>null</c>.</param>
        public void Connect(string hostname, int port = 6667, IrcClientConnectionOptions options = null)
        {
            Throw.If.Null(hostname, "hostname").Negative(port, "port");

            Connect(client => client.Connect(hostname, port), options);
        }

        /// <summary>
        /// Connects to an IRC server specified by an endpoint.
        /// </summary>
        /// <param name="endPoint">The IP endpoint to connect to.</param>
        /// <param name="options">Options for the connection, if any, or <c>null</c>.</param>
        public void Connect(IPEndPoint endPoint, IrcClientConnectionOptions options = null)
        {
            Throw.If.Null(endPoint, "endPoint");

            Connect(client => client.Connect(endPoint), options);
        }

        void Connect(Action<TcpClient> connectTcpClientCallback, IrcClientConnectionOptions options)
        {
            var client = new TcpClient();
            connectTcpClientCallback(client);

            try
            {
                Connect(client.GetStream(), options);
            }
            catch (Exception)
            {
                client.Close(); throw;
            }
        }

        /// <summary>
        /// Connects to an IRC server specified by a stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="options">Options for the connection, if any, or <c>null</c>.</param>
        public void Connect(Stream stream, IrcClientConnectionOptions options = null)
        {
            Throw.If.Null(stream, "stream");

            Close();

            if (options == null)
            {
                options = new IrcClientConnectionOptions();
            }

            if (options.Ssl)
            {
                if (options.SslHostname == null)
                {
                    throw new ArgumentException("If Ssl is true, SslHostname must be set.", "options");
                }

                var sslStream = new SslStream(stream, false, options.SslCertificateValidationCallback);
                sslStream.AuthenticateAsClient(options.SslHostname);
                stream = sslStream;
            }

            var context = new Context()
            {
                ReceiverThread = new Thread(ThreadReceiver)
                {
                    Name = "IRC Receiver",
                },
                StartEvent = new ManualResetEvent(false),
                Stream = stream,
                SynchronizationContext = options.SynchronizationContext
            };

            _context = context;
            context.ReceiverThread.IsBackground = true;
            context.ReceiverThread.Start(context);

            try
            {
                IsConnected = true; OnConnected();
            }
            finally
            {
                context.StartEvent.Set();
            }
        }

        /// <summary>
        /// Sends a CTCP command to the specified user or channel.
        /// </summary>
        /// <param name="recipient">The user or channel to send the command to.</param>
        /// <param name="command">The CTCP command.</param>
        /// <param name="parameters">The CTCP command parameters.</param>
        /// <param name="escapeParameters">
        ///     <c>true</c> to quote parameters with spaces in them, and escape backslashes and quotation marks.
        /// </param>
        public void CtcpCommand(IrcString recipient, IrcString command,
                                IrcString[] parameters, bool escapeParameters = true)
        {
            Throw.If.Null(recipient, "recipient").Null(command, "command").NullElements(parameters, "parameters");

            Message(recipient, CtcpEncode(command, parameters, escapeParameters));
        }

        /// <summary>
        /// Replies to a CTCP command from a user or channel.
        /// </summary>
        /// <param name="recipient">The user or channel to send the reply to.</param>
        /// <param name="command">The CTCP command.</param>
        /// <param name="parameters">The CTCP command reply parameters.</param>
        /// <param name="escapeParameters">
        ///     <c>true</c> to quote parameters with spaces in them, and escape backslashes and quotation marks.
        /// </param>
        public void CtcpReply(IrcString recipient, IrcString command,
                              IrcString[] parameters, bool escapeParameters = true)
        {
            Throw.If.Null(recipient, "recipient").Null(command, "command").NullElements(parameters, "parameters");

            Notice(recipient, CtcpEncode(command, parameters, escapeParameters));
        }

        /// <summary>
        /// Sends a DCC command to the specified user or channel.
        /// </summary>
        /// <param name="recipient">The user or channel to send the command to.</param>
        /// <param name="command">The DCC command.</param>
        /// <param name="parameters">The DCC command parameters.</param>
        public void DccCommand(IrcString recipient, IrcString command, params IrcString[] parameters)
        {
            Throw.If.Null(recipient, "recipient").Null(command, "command").NullElements(parameters, "parameters");

            CtcpCommand(recipient, "DCC", new[] { command }.Concat(parameters).ToArray());
        }

        /// <summary>
        /// Constructs and sends an IRC command to the server.
        /// </summary>
        /// <param name="command">The command to send.</param>
        /// <param name="parameters">The command's parameters.</param>
        /// <returns><c>true</c> if the command was sent successfully.</returns>
        public bool IrcCommand(IrcString command, params IrcString[] parameters)
        {
            Throw.If.Null(command, "command").Null(parameters, "parameters");

            return IrcCommand(new IrcStatement(null, command, parameters));
        }

        /// <summary>
        /// Sends a premade IRC statement to the server.
        /// </summary>
        /// <param name="statement">The statement to send.</param>
        /// <returns><c>true</c> if the statement was sent successfully.</returns>
        public bool IrcCommand(IrcStatement statement)
        {
            Throw.If.Null(statement, "statement");

            var buffer = statement.ToByteArray();
            lock (SyncRoot)
            {
#if DEBUG
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("< " + new IrcString(buffer));
                Console.ResetColor();
#endif

                var context = _context;
                if (context == null) { return false; }

                try
                {
                    context.Stream.Write(buffer, 0, buffer.Length);
                }
                catch (IOException)
                {
                    return false;
                }
                catch (ObjectDisposedException)
                {
                    return false;
                }

                return true;
            }
        }

        /// <summary>
        /// Closes the network connection.
        /// </summary>
        public void Close()
        {
            var context = _context;
            if (context == null) { return; }

            context.Stream.Close();
            context.ReceiverThread.Join();
            _context = null;
        }

        void ThreadReceiver(object parameter)
        {
            var context = (Context)parameter;
            var receiver = new IrcStatementReceiver(context.Stream);
            context.StartEvent.Set();

            while (true)
            {
                IrcStatement statement; IrcStatementParseResult parseResult;

                if (receiver.TryReceive(out statement, out parseResult))
                {
                    OnIrcStatementReceived(statement);
                }
                else if (parseResult != IrcStatementParseResult.NothingToParse)
                {
                    break;
                }
            }

            IsConnected = false; OnClosed();
        }

        /// <summary>
        /// The client version. This will be sent in reply to a CTCP VERSION query.
        /// </summary>
        [AmbientValue(null)]
        public IrcString ClientVersion
        {
            get { return _clientVer ?? ("NetIRC2 " + Assembly.GetExecutingAssembly().GetName().Version.ToString()); }
            set { _clientVer = value; }
        }

        bool ShouldSerializeClientVersion()
        {
            return _clientVer != null;
        }

        /// <summary>
        /// Whether the client is connected to a server.
        /// </summary>
        [Browsable(false)]
        public bool IsConnected
        {
            get;
            private set;
        }

        /// <summary>
        /// The synchronization object for sending IRC commands.
        /// </summary>
        [Browsable(false)]
        public object SyncRoot
        {
            get;
            private set;
        }
    }
}
