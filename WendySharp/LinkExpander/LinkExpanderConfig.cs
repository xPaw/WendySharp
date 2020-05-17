using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace WendySharp
{
    class LinkExpanderConfig
    {
        /// <summary>
        /// Get it from https://apps.twitter.com/
        /// </summary>
        public class TwitterConfig
        {
            [JsonProperty(Required = Required.Always)]
            public string AccessToken { get; set; }

            [JsonProperty(Required = Required.Always)]
            public string AccessSecret { get; set; }

            [JsonProperty(Required = Required.Always)]
            public string ConsumerKey { get; set; }

            [JsonProperty(Required = Required.Always)]
            public string ConsumerSecret { get; set; }

            /// <summary>
            /// Whether or not to expand URLs in tweets.
            /// </summary>
            [JsonProperty(Required = Required.Always)]
            public bool ExpandURLs { get; set; }

            [JsonProperty(Required = Required.Always)]
            public Dictionary<string, List<string>> AccountsToFollow { get; set; }
        }

        public class YoutubeConfig
        {
            [JsonProperty(Required = Required.Always)]
            public string ApiKey { get; set; }
        }

        [JsonProperty(Required = Required.Always)]
        public TwitterConfig Twitter { get; set; }

        [JsonProperty(Required = Required.Always)]
        public YoutubeConfig YouTube { get; set; }

        /// <summary>
        /// Don't repeat a tweet if it was already mentioned in the last N tweets.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public uint DontRepeatLastCount { get; set; }

        [JsonProperty(Required = Required.Always)]
        public List<string> Channels;
    }
}
