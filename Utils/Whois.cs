using System;
using NetIrc2;
using NetIrc2.Events;
using System.Collections.Generic;

namespace WendySharp
{
    class WhoisData
    {
        public IrcIdentity Identity;
        public IrcString Account;
        public List<Action<WhoisData>> Callbacks;

        public WhoisData()
        {
            Identity = new IrcIdentity();
            Callbacks = new List<Action<WhoisData>>();
        }
    }

    class Whois
    {
        private readonly Dictionary<string, WhoisData> Pending;

        public Whois(IrcClient client)
        {
            Pending = new Dictionary<string, WhoisData>();

            client.GotIrcError += OnIrcError;
            client.GotUnknownIrcStatement += OnIrcStatement;
        }

        public void Query(IrcIdentity ident, Action<WhoisData> callback)
        {
            if (ident.Username != null || ident.Hostname != null)
            {
                var data = new WhoisData();
                data.Identity = ident;

                callback.Invoke(data);
            }
            else
            {
                QueryAccount(ident, callback);
            }
        }

        public void QueryAccount(IrcIdentity ident, Action<WhoisData> callback)
        {
            WhoisData data;

            if (Pending.ContainsKey(ident.Nickname))
            {
                data = Pending[ident.Nickname];
            }
            else
            {
                data = new WhoisData();

                Pending.Add(ident.Nickname, data);

                // TODO: create a timeout?
            }

            data.Callbacks.Add(callback);

            Bootstrap.Client.Client.Whois(ident.Nickname);
        }

        public static IrcIdentity NormalizeIdentity(IrcIdentity ident)
        {
            ident.Nickname = "*";

            if (ident.Username == null)
            {
                ident.Username = "*";
            }

            if (ident.Hostname == null)
            {
                ident.Hostname = "*";
            }
            else if (ident.Hostname.StartsWith("gateway/web/freenode/ip."))
            {
                var temp = ident.Hostname.ToString().Split('/');

                ident.Hostname = temp[3].Substring(3);
                //ident.Nickname = "*";
                ident.Username = "*";
            }
            else if (ident.Hostname.StartsWith("gateway/"))
            {
                //ident.Nickname = "*";
                ident.Hostname = "gateway/*";
            }

            return ident;
        }

        private void OnIrcStatement(object sender, IrcUnknownStatementEventArgs e)
        {
            var @params = e.Data.Parameters;
            IrcString nickname;

            switch ((string)e.Data.Command)
            {
                case "311":
                    if (@params.Count >= 4)
                    {
                        nickname = @params[1];

                        if (Pending.ContainsKey(nickname))
                        {
                            Pending[nickname].Identity.Nickname = nickname;
                            Pending[nickname].Identity.Username = @params[2];
                            Pending[nickname].Identity.Hostname = @params[3];
                        }
                    }

                    break;

                case "330":
                    if (@params.Count >= 3 && Pending.ContainsKey(@params[1]))
                    {
                        Pending[@params[1]].Account = @params[2];
                    }

                    break;

                case "318":
                    WhoisData data;

                    nickname = @params[1];

                    if (!Pending.TryGetValue(nickname, out data))
                    {
                        break;
                    }

                    Pending.Remove(nickname);

                    foreach (var callback in data.Callbacks)
                    {
                        callback.Invoke(data);
                    }

                    break;
            }
        }

        private void OnIrcError(object sender, IrcErrorEventArgs e)
        {
            if (e.Error == IrcReplyCode.NoSuchNickname)
            {
                var nick = e.Data.Parameters[1];

                WhoisData data;

                if (!Pending.TryGetValue(nick, out data))
                {
                    return;
                }

                Pending.Remove(nick);

                foreach (var callback in data.Callbacks)
                {
                    callback.Invoke(data);
                }
            }
        }
    }
}
