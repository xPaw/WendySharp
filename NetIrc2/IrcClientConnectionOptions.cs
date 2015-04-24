using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Threading;
using System.Text;

namespace NetIrc2
{
    /// <summary>
    /// Provides options used by the <see cref="IrcClient.Connect(string, int, IrcClientConnectionOptions)"/> command.
    /// </summary>
    public class IrcClientConnectionOptions
    {
        public IrcClientConnectionOptions()
        {
            SynchronizationContext = SynchronizationContext.Current;
        }

        /// <summary>
        /// <c>true</c> to use SSL.
        /// </summary>
        public bool Ssl
        {
            get;
            set;
        }

        /// <summary>
        /// The SSL hostname. You must set this if you are using SSL.
        /// </summary>
        public string SslHostname
        {
            get;
            set;
        }

        /// <summary>
        /// The SSL certificate validation callback.
        /// If you are using SSL, the callback is not set, and the SSL certificate is invalid, the connection will fail.
        /// This can be used to offer the user an option to connect despite the invalid SSL certificate.
        /// </summary>
        public RemoteCertificateValidationCallback SslCertificateValidationCallback
        {
            get;
            set;
        }

        /// <summary>
        /// The synchronization context for the connection.
        /// If this is <c>null</c>, IRC events will be raised in an arbitrary thread.
        /// If this is not <c>null</c>, all IRC events will be posted to the synchronization context.
        /// By default, the synchronization context is captured at construction time.
        /// 
        /// What this means: If you create and use the IRC client from a Windows Forms, WPF,
        /// or other GUI thread, events will be raised on the same thread and you will not have to worry
        /// about multithreading at all.
        /// </summary>
        public SynchronizationContext SynchronizationContext
        {
            get;
            set;
        }
    }
}
