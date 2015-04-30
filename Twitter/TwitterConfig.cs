using System;

namespace WendySharp
{
    class TwitterConfig
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

        /// <summary>
        /// Don't repeat a tweet if it was already mentioned in the last N tweets.
        /// </summary>
        public uint DontRepeatLastCount { get; set; }
    }
}
