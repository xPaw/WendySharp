using System.Collections.Generic;

namespace WendySharp
{
    class LinkExpanderConfig
    {
        public class TwitterConfig
        {
            // https://apps.twitter.com/
            public string AccessToken { get; set; }
            public string AccessSecret { get; set; }
            public string ConsumerKey { get; set; }
            public string ConsumerSecret { get; set; }

            /// <summary>
            /// Whether or not to expand URLs in tweets.
            /// </summary>
            public bool ExpandURLs { get; set; }
        }

        public TwitterConfig Twitter { get; set; }

        /// <summary>
        /// Don't repeat a tweet if it was already mentioned in the last N tweets.
        /// </summary>
        public uint DontRepeatLastCount { get; set; }

        public List<string> Channels;
    }
}
