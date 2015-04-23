using System;
using NetIrc2;
using NetIrc2.Events;
using System.Threading;
using System.Threading.Tasks;

namespace WendySharp
{
    class BaseClient
    {
        public readonly IrcClient Client;
        public ModeList ModeList;
        public readonly Channels ChannelList;
        public string TrueNickname;

        public BaseClient()
        {
            Client = new IrcClient();

            Client.ClientVersion = "Wendy";

            Client.Connected += OnConnected;
            Client.Closed += OnDisconnected;
            Client.GotIrcError += OnError;

            ChannelList = new Channels(Client);

            new Permissions();
            new Commands(Client);
        }

        public void Connect()
        {
            TrueNickname = Settings.BotNick;

            try
            {
                var options = new IrcClientConnectionOptions();
                options.SynchronizationContext = SynchronizationContext.Current;

                Client.Connect("irc.freenode.net", 6665);
            }
            catch (Exception e)
            {
                Log.WriteError("IRC", "Failed to connect: {0}\n{1}", e.Message, e.StackTrace);
            }
        }

        public void Close()
        {
            Client.LogOut();
            Client.Close();
        }

        private void OnConnected(object sender, EventArgs e)
        {
            Log.WriteInfo("IRC", "Connected to IRC successfully");

            Client.LogIn("WendySharp", "WendySharp", "WendySharp", "4", null, null);
            Client.Join("#xpaw-test");

            if (ModeList == null)
            {
                ModeList = new ModeList(Client);
            }
        }

        private async void OnDisconnected(object sender, EventArgs e)
        {
            Log.WriteInfo("IRC", "Disconnected from IRC");

            if (!Bootstrap.ResetEvent.WaitOne(0))
            {
                await Task.Delay(2000);

                Log.WriteInfo("IRC", "Reconnecting");

                Connect();
            }
            else
            {
                Log.WriteInfo("IRC", "Exiting");
            }
        }

        private void OnError(object sender, IrcErrorEventArgs e)
        {
            switch (e.Error)
            {
                case IrcReplyCode.MissingMOTD:
                    return;

                case IrcReplyCode.NotChannelOperator:

                    return;
            }

            Log.WriteError("IRC", "Error: {0} ({1})", e.Error.ToString(), string.Join(", ", e.Data.Parameters));
        }

        public void SendReply(string recipient, string message, bool notice)
        {
            if (notice)
            {
                Client.Notice(recipient, message);
            }
            else
            {
                Client.Message(recipient, message);
            }
        }
    }
}
