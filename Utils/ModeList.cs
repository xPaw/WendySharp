using System;
using System.Collections.Generic;
using System.Timers;
using LitJson;
using NetIrc2;
using NetIrc2.Events;

namespace WendySharp
{
    class ModeList
    {
        private readonly List<Timer> Timers;

        public ModeList(IrcClient client)
        {
            Timers = new List<Timer>();

            client.GotMode += OnModeChange;
        }

        private void OnModeChange(object sender, ModeEventArgs e)
        {
            Log.WriteDebug("Mode", "{0} ({1} parameters)", e.Command, e.ParameterCount);

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

                //parameters[index++];

                index++;
            }
        }
    }
}
