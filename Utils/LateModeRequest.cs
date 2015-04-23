using System;
using NetIrc2;

namespace WendySharp
{
    class LateModeRequest
    {
        public string Channel { get; set; }
        public string Recipient { get; set; }
        public string Reason { get; set; }
        public DateTime Time { get; set; }
        public string Mode { get; set; }

        public void Execute(IrcClient client)
        {
            if (!client.IsConnected)
            {
                return;
            }

            client.Mode(Channel, Mode, new IrcString[1] { Recipient });
        }
    }
}
