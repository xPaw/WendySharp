using System;
using System.Collections.Generic;
using System.IO;
using LitJson;
using NetIrc2;
using NetIrc2.Events;

namespace WendySharp
{
    class ModeList
    {
        private readonly List<LateModeRequest> LateModes;
        private readonly string FilePath;

        public ModeList(IrcClient client)
        {
            LateModes = new List<LateModeRequest>();

            FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config", "modes.json");

            if (File.Exists(FilePath))
            {
                using (var file = new StreamReader(FilePath))
                {
                    LateModes = JsonMapper.ToObject<List<LateModeRequest>>(file);
                }

                Log.WriteInfo("ModeList", "Loaded {0} modes from file", LateModes.Count);

                RecheckLateModes();
            }

            client.GotMode += OnModeChange;
        }

        public void RecheckLateModes()
        {
            foreach (var mode in LateModes)
            {
                mode.Check();
            }
        }

        public List<LateModeRequest> GetModes()
        {
            return LateModes;
        }

        public LateModeRequest Find(string channel, string recipient, string mode)
        {
            return LateModes.Find(x =>
                x.Channel == channel &&
                x.Recipient == recipient &&
                x.Mode == mode
            );
        }

        public void AddLateModeRequest(LateModeRequest mode)
        {
            mode.Check();

            LateModes.Add(mode);

            SaveToFile();
        }

        public void RemoveLateModeRequest(LateModeRequest request)
        {
            request.Dispose();

            LateModes.Remove(request);

            SaveToFile();
        }

        private void OnModeChange(object sender, ModeEventArgs e)
        {
            // usermode?
            if (e.ParameterCount == 0)
            {
                return;
            }

            var command = e.Command.ToString();
            var parameters = e.GetParameterList();
            char currentState = ' ';
            int index = 0;
            string ident;

            for (int i = 0; i < command.Length; i++)
            {
                if (command[i] == '+' || command[i] == '-')
                {
                    currentState = command[i];

                    continue;
                }

                ident = parameters[index++];

                Log.WriteDebug("Mode", "{0}{1} on {2}", currentState, command[i], ident);

                var channel = Bootstrap.Client.ChannelList.GetChannel(e.Recipient);

                if (command[i] == 'o')
                {
                    if (currentState == '+')
                    {
                        channel.Users[ident] = Channel.Operator;

                        if (ident == Bootstrap.Client.TrueNickname)
                        {
                            // We gained op, maybe we can remove some bans now
                            RecheckLateModes();
                        }
                    }
                    else
                    {
                        channel.Users[ident] = 0;
                    }
                }
                else if (command[i] == 'b')
                {
                    // Drop channel forward
                    var temp = ident.Split(new char[] { '$' }, 2);

                    if (temp.Length > 1)
                    {
                        ident = temp[0];
                    }
                }

                var mode = Find(e.Recipient, ident, string.Format("{0}{1}", currentState, command[i]));

                if (mode != null)
                {
                    Log.WriteInfo("Mode", "'{0}' in {1} was set {2}{3}, removing our late mode request", ident, e.Recipient, currentState, command[i]);

                    RemoveLateModeRequest(mode);
                }
            }
        }

        private void SaveToFile()
        {
            var writer = new JsonWriter();
            writer.PrettyPrint = true;

            JsonMapper.ToJson(LateModes, writer);

            File.WriteAllText(FilePath, writer.ToString());
        }
    }
}
