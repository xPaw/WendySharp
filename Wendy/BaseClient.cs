using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using LitJson;
using NetIrc2;
using NetIrc2.Events;
using NetIrc2.Parsing;

namespace WendySharp
{
    class BaseClient
    {
        public readonly IrcClient Client;
        public readonly Settings Settings;
        public readonly Channels ChannelList;
        public string TrueNickname;
        public ModeList ModeList;
        public Whois Whois;

        public BaseClient()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config", "settings.json");

            if (File.Exists(path))
            {
                var data = File.ReadAllText(path);

                try
                {
                    Settings = JsonMapper.ToObject<Settings>(data);

                    if (!IrcValidation.IsNickname(Settings.Nickname))
                    {
                        throw new JsonException("Nickname is not valid.");
                    }

                    foreach (var channel in Settings.Channels)
                    {
                        if (!IrcValidation.IsChannelName(channel))
                        {
                            throw new JsonException(string.Format("Channel '{0}' is not valid.", channel));
                        }
                    }
                }
                catch (JsonException e)
                {
                    Log.WriteError("IRC", "Failed to parse settings.json file: {0}", e.Message);

                    Environment.Exit(1);
                }
            }
            else
            {
                Log.WriteWarn("IRC", "File config/settings.json doesn't exist");

                Environment.Exit(1);
            }

            Client = new IrcClient();

            Client.ClientVersion = "Wendy# -- https://github.com/xPaw/WendySharp";

            Client.Connected += OnConnected;
            Client.Closed += OnDisconnected;
            Client.GotIrcError += OnError;

            ChannelList = new Channels(Client);

            new Permissions();
            new Commands(Client);
            new Spam(Client);
            Whois = new Whois(Client);
        }

        public void Connect()
        {
            TrueNickname = Settings.Nickname;

            var options = new IrcClientConnectionOptions();
            options.SynchronizationContext = SynchronizationContext.Current;

            Client.Connect(Settings.Server, Settings.Port, options);
        }

        public void Close()
        {
            Client.LogOut();
            Client.Close();
        }

        private void OnConnected(object sender, EventArgs e)
        {
            Log.WriteInfo("IRC", "Connected to IRC successfully");

            Client.LogIn(Settings.Nickname, Settings.Nickname, Settings.Nickname, null, null, Settings.Password);

            foreach (var channel in Settings.Channels)
            {
                Client.Join(channel);
            }

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
