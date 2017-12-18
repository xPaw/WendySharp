using System.Collections.Generic;
using Newtonsoft.Json;

namespace WendySharp
{
    class Settings
    {
        [JsonProperty(Required = Required.Always)]
        public string Server { get; set; }

        [JsonProperty(Required = Required.Always)]
        public int Port { get; set; }

        [JsonProperty(Required = Required.AllowNull)]
        public string Password { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string Nickname { get; set; }

        [JsonProperty(Required = Required.Always)]
        public char Prefix { get; set; }

        [JsonProperty(Required = Required.Always)]
        public bool UsesChanServ { get; set; } // Needs +o on ChanServ

        [JsonProperty(Required = Required.Always)]
        public string RedirectChannel { get; set; }

        [JsonProperty(Required = Required.Always)]
        public List<string> Channels { get; set; }
    }
}
