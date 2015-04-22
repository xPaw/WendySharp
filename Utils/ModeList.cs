using System;
using System.Collections.Generic;
using System.IO;
using System.Timers;
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
            }

            client.GotMode += OnModeChange;
        }

        public void AddLateModeRequest(LateModeRequest request)
        {
            // TODO: start a timer or some other magic

            LateModes.Add(request);

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

            for (int i = 0; i < command.Length; i++)
            {
                if (command[i] == '+' || command[i] == '-')
                {
                    currentState = command[i];

                    continue;
                }

                Log.WriteDebug("Mode", "{0}{1} on {2}", currentState, command[i], parameters[index]);

                if (parameters[index] == Bootstrap.Client.TrueNickname)
                {
                    if (command[i] == 'o')
                    {
                        var channel = Bootstrap.Client.ChannelList.GetChannel(e.Recipient);

                        channel.WeAreOpped = currentState == '+';

                        Log.WriteDebug("Mode", "Updated our op status in {0}", channel.Name);
                    }
                }

                index++;
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
