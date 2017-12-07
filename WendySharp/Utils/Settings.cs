using System.Collections.Generic;

namespace WendySharp
{
    class Settings
    {
        public string Server { get; set; }
        public int Port { get; set; }
        public string Password { get; set; }
        public string Nickname { get; set; }
        public char Prefix { get; set; }
        public bool UsesChanServ { get; set; } // Needs +o on ChanServ
        public string RedirectChannel { get; set; }
        public List<string> Channels { get; set; }
    }
}
