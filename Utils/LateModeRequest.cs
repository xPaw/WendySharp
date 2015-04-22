using System;

namespace WendySharp
{
    class LateModeRequest
    {
        public string Channel { get; set; }
        public string Recipient { get; set; }
        public string Reason { get; set; }
        public DateTime Time { get; set; }
        public string Mode { get; set; }
    }
}
