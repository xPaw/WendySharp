using System;
using System.Threading;
using System.IO;

namespace WendySharp
{
    class Bootstrap
    {
        public static ManualResetEvent ResetEvent { get; private set; }
        public static BaseClient Client { get; private set; }

        public static void Main(string[] args)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config", "bugsnag.txt");

            if (File.Exists(path))
            {
                new Bugsnag.Clients.BaseClient(File.ReadAllText(path).Trim());
            }

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
    }
}
