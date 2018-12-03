using System;
using System.Threading;

namespace WendySharp
{
    static class Bootstrap
    {
        public static ManualResetEvent ResetEvent { get; private set; }
        public static BaseClient Client { get; private set; }

        public static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

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

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            var e = args.ExceptionObject as Exception;

            Log.WriteError("Unhandled Exception", "{0}\n{1}", e?.Message, e?.StackTrace);

            if (args.IsTerminating)
            {
                AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;

                ResetEvent.Set();
            }
        }
    }
}
