using System;
using System.Collections.Generic;
using NetIrc2;
using NetIrc2.Events;

namespace WendySharp
{
    class WhoisData
    {
        public IrcIdentity Identity;
        public IrcString Account;
        public readonly List<Action<WhoisData>> Callbacks;

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
            Pending = new Dictionary<string, WhoisData>(StringComparer.InvariantCultureIgnoreCase);

            client.GotIrcError += OnIrcError;
            client.GotUnknownIrcStatement += OnIrcStatement;
        }

        public void Query(IrcIdentity ident, Action<WhoisData> callback)
        {
            if (ident.Username != null || ident.Hostname != null)
            {
                var data = new WhoisData
                {
                    Identity = ident
                };

                callback.Invoke(data);
            }
            else
            {
                QueryAccount(ident, callback);
            }
        }

        private void QueryAccount(IrcIdentity ident, Action<WhoisData> callback)
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

        public static IrcIdentity NormalizeIdentity(IrcIdentity originalIdent)
        {
            var ident = new IrcIdentity
            {
                Hostname = originalIdent.Hostname,
                Nickname = originalIdent.Nickname,
                Username = originalIdent.Username,
            };

            if (ident.Username == null)
            {
                ident.Username = "*";
            }

            if (ident.Hostname == null)
            {
                ident.Hostname = "*";
            }
            else if (ident.Hostname != "*")
            {
                //ident.Username = "*";
            }

            // We don't want to accidentally ban *!*@*
            if (ident.Username != "*" || ident.Hostname != "*")
            {
                ident.Nickname = "*";
            }

            return ident;
        }

        private void OnIrcStatement(object sender, IrcUnknownStatementEventArgs e)
        {
            var @params = e.Data.Parameters;
            IrcString nickname;

            switch (e.Data.Command)
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

                    nickname = @params[1];

                    if (!Pending.TryGetValue(nickname, out var data))
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
            if (e.Error != IrcReplyCode.NoSuchNickname)
            {
                return;
            }

            var nickname = e.Data.Parameters[1];

            if (!Pending.TryGetValue(nickname, out var data))
            {
                return;
            }

            Pending.Remove(nickname);

            foreach (var callback in data.Callbacks)
            {
                callback.Invoke(data);
            }
        }
    }
}
