using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NetIrc2;
using NetIrc2.Parsing;
using Newtonsoft.Json;

namespace WendySharp
{
    class BaseClient
    {
        public readonly IrcClient Client;
        public readonly Settings Settings;
        public readonly Channels ChannelList;
        public string TrueNickname;
        public ModeList ModeList;
        public readonly Whois Whois;
        private readonly LinkExpander LinkExpander;

        public BaseClient()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config", "settings.json");

            if (File.Exists(path))
            {
                var data = File.ReadAllText(path);

                try
                {
                    Settings = JsonConvert.DeserializeObject<Settings>(data, new JsonSerializerSettings { MissingMemberHandling = MissingMemberHandling.Error });

                    if (!IrcValidation.IsNickname(Settings.Nickname))
                    {
                        throw new JsonException("Nickname is not valid.");
                    }

                    if (!IrcValidation.IsChannelName(Settings.RedirectChannel))
                    {
                        throw new JsonException("Redirect channel is not valid.");
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

            Client = new IrcClient
            {
                ClientVersion = "Wendy# -- https://github.com/xPaw/WendySharp"
            };

            Client.Connected += OnConnected;
            Client.Closed += OnDisconnected;

            new Permissions();
            new Commands(Client);
            LinkExpander = new LinkExpander(Client);
            ChannelList = new Channels(Client);
            Whois = new Whois(Client);
        }

        public void Connect()
        {
            TrueNickname = Settings.Nickname;

            var options = new IrcClientConnectionOptions
            {
                SynchronizationContext = SynchronizationContext.Current
            };

            Client.Connect(Settings.Server, Settings.Port, options);
        }

        public void Close()
        {
            Client.LogOut();
            Client.Close();

            LinkExpander.TwitterStream?.StopStream();
        }

        private void OnConnected(object sender, EventArgs e)
        {
            Log.WriteInfo("IRC", "Connected successfully");

            Client.LogIn(Settings.Nickname, "https://github.com/xPaw/WendySharp", Settings.Nickname, null, null, Settings.Password);

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
            Log.WriteInfo("IRC", "Disconnected");

            if (!Bootstrap.ResetEvent.WaitOne(0))
            {
                await Task.Delay(2000).ConfigureAwait(false);

                Log.WriteInfo("IRC", "Reconnecting");

                Connect();
            }
            else
            {
                Log.WriteInfo("IRC", "Exiting");
            }
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
