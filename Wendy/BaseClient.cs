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
        public readonly ModeList ModeList;

        public BaseClient()
        {
            Client = new IrcClient();

            Client.ClientVersion = "Wendy";

            Client.Connected += OnConnected;
            Client.Closed += OnDisconnected;
            Client.GotIrcError += OnError;
            Client.GotChannelListEntry += OnGotChannelListEntry;
            Client.GotNameListReply += OnGotNameListReply;

            ModeList = new ModeList(Client);

            new Permissions();
            new Commands(Client);
        }

        public void Connect()
        {
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

        private void OnGotChannelListEntry(object sender, ChannelListEntryEventArgs e)
        {
            Log.WriteDebug("OnGotChannelListEntry", "{0} - {1} - {2}", e.Channel, e.Topic, e.UserCount);
        }

        private void OnGotNameListReply(object sender, NameListReplyEventArgs e)
        {
            var names = e.GetNameList();

            foreach (var name in names)
            {
                Log.WriteDebug("OnGotNameListReply", "{0} - {1}", e.Channel, name);
            }
        }

        private void OnError(object sender, IrcErrorEventArgs e)
        {
            switch (e.Error)
            {
                case IrcReplyCode.MissingMOTD:
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
