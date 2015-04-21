using System;
using System.Threading;
using System.Linq;

namespace WendySharp
{
    class Bootstrap
    {
        public static ManualResetEvent ResetEvent { get; private set; }
        public static BaseClient Client { get; private set; }

        public static void Main(string[] args)
        {
            ResetEvent = new ManualResetEvent(false);

            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                ResetEvent.Set();
            };
            
            Client = new BaseClient();
            Client.Connect();

            ResetEvent.WaitOne();

            Client.Close();
        }

        private static readonly char[] ChannelCharacters = { '#', '!', '+', '&' };

        public static bool IsRecipientChannel(string recipient)
        {
            return ChannelCharacters.Contains(recipient[0]);
        }
    }
}
