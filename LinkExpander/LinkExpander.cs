using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using LitJson;
using NetIrc2;
using NetIrc2.Events;
using NetIrc2.Parsing;

namespace WendySharp
{
    class LinkExpander
    {
        private readonly Regex TwitterCompiledMatch;
        private readonly Regex YoutubeCompiledMatch;
        private readonly Regex TwitchCompiledMatch;
        private readonly FixedSizedQueue<string> LastMatches;
        private readonly LinkExpanderConfig Config;

        public LinkExpander(IrcClient client)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config", "services.json");

            if (!File.Exists(path))
            {
                Log.WriteWarn("Twitter", "File config/services.json doesn't exist");

                return;
            }

            var data = File.ReadAllText(path);

            try
            {
                Config = JsonMapper.ToObject<LinkExpanderConfig>(data);

                if (Config.Twitter.AccessSecret == null ||
                    Config.Twitter.AccessToken == null ||
                    Config.Twitter.ConsumerKey == null ||
                    Config.Twitter.ConsumerSecret == null)
                {
                    throw new JsonException("Twitter keys cannot be null");
                }

                if (Config.Channels == null)
                {
                    Config.Channels = new List<string>();
                }
                else
                {
                    foreach (var channel in Config.Channels)
                    {
                        if (!IrcValidation.IsChannelName(channel))
                        {
                            throw new JsonException(string.Format("Invalid channel '{0}'", channel));
                        }
                    }
                }
            }
            catch (JsonException e)
            {
                Log.WriteError("Twitter", "Failed to parse services.json file: {0}", e.Message);

                Environment.Exit(1);
            }

            LastMatches = new FixedSizedQueue<string>();
            LastMatches.Limit = Config.DontRepeatLastCount;

            TwitterCompiledMatch = new Regex(@"(^|/|\.)twitter\.com/(.+?)/status/(?<status>[0-9]+)", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);
            YoutubeCompiledMatch = new Regex(@"(^|/|\.)(youtube\.com/watch\?v=|youtube\.com/embed/|youtu\.be/)(?<id>[a-zA-Z0-9\-_]+)", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);
            TwitchCompiledMatch = new Regex(@"(^|/|\.)twitch\.tv/(?<channel>[a-zA-Z0-9_]+)", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);

            client.GotMessage += OnMessage;
        }

        private void OnMessage(object sender, ChatMessageEventArgs e)
        {
            if (!Config.Channels.Contains(e.Recipient))
            {
                return;
            }

            ProcessTwitter(e);
            ProcessYoutube(e);
            ProcessTwitch(e);
        }

        private void ProcessTwitter(ChatMessageEventArgs e)
        {
            var matches = TwitterCompiledMatch.Matches(e.Message);

            foreach (Match match in matches)
            {
                var status = match.Groups["status"].Value;

                if (LastMatches.Contains(status))
                {
                    continue;
                }

                LastMatches.Enqueue(status);

                using (var webClient = new SaneWebClient())
                {
                    var url = string.Format("https://api.twitter.com/1.1/statuses/show/{0}.json", status);
                    var authHeader = TwitterAuthorization.GetHeader("GET", url, Config.Twitter);

                    webClient.DownloadDataCompleted += (s, twitter) =>
                    {
                        if (twitter.Error != null || twitter.Cancelled)
                        {
                            Log.WriteError("Twitter", "Exception: {0}", twitter.Error.Message);
                            return;
                        }

                        var response = Encoding.UTF8.GetString(twitter.Result);
                        var tweet = JsonMapper.ToObject(response);

                        if (tweet["text"] == null)
                        {
                            return;
                        }

                        var text = WebUtility.HtmlDecode(tweet["text"].ToString()).Replace('\n', ' ').Trim();

                        // Check if original message contains tweet text (with t.co links)
                        if (e.Message.ToString().Contains(text))
                        {
                            return;
                        }

                        if (Config.Twitter.ExpandURLs)
                        {
                            foreach (JsonData entityUrl in tweet["entities"]["urls"])
                            {
                                text = text.Replace(WebUtility.HtmlDecode((string)entityUrl["url"]), WebUtility.HtmlDecode((string)entityUrl["expanded_url"]));
                            }

                            // Check if original message contains tweet text (with expanded links)
                            if (e.Message.ToString().Contains(text))
                            {
                                return;
                            }
                        }

                        DateTime date;

                        if (!DateTime.TryParseExact(tweet["created_at"].ToString(), "ddd MMM dd HH:mm:ss zz00 yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out date))
                        {
                            date = DateTime.UtcNow;
                        }

                        Bootstrap.Client.Client.Message(e.Recipient,
                            string.Format("{0}» {1}{2}{3} {4}{5}: {6}",
                                Color.OLIVE,
                                Color.BLUE,
                                tweet["user"]["name"],
                                Color.LIGHTGRAY,
                                date.ToRelativeString(),
                                Color.NORMAL,
                                text
                            )
                        );

                        var fakeEvent = new ChatMessageEventArgs(e.Sender, e.Recipient, text);

                        ProcessYoutube(fakeEvent);
                        ProcessTwitch(fakeEvent);
                    };

                    webClient.Headers.Add(HttpRequestHeader.Authorization, string.Format("OAuth {0}", authHeader));
                    webClient.DownloadDataAsync(new Uri(url));
                }
            }
        }

        private void ProcessYoutube(ChatMessageEventArgs e)
        {
            var matches = YoutubeCompiledMatch.Matches(e.Message);

            foreach (Match match in matches)
            {
                var id = match.Groups["id"].Value;

                if (LastMatches.Contains(id))
                {
                    continue;
                }

                LastMatches.Enqueue(id);

                using (var webClient = new SaneWebClient())
                {
                    webClient.DownloadDataCompleted += (s, youtube) =>
                    {
                        if (youtube.Error != null || youtube.Cancelled)
                        {
                            return;
                        }

                        var response = Encoding.UTF8.GetString(youtube.Result);
                        var data = JsonMapper.ToObject(response);

                        if (data["items"] == null || data["items"].Count == 0)
                        {
                            return;
                        }

                        var item = data["items"][0];

                        // If original message already contains video title, don't post it again
                        if (e.Message.ToString().Contains(item["snippet"]["title"].ToString()))
                        {
                            return;
                        }

                        var info = string.Empty;
                        var time = XmlConvert.ToTimeSpan(item["contentDetails"]["duration"].ToString());
                        var duration = time == TimeSpan.Zero ? string.Empty : string.Format(" ({0})", time);
                            
                        if (item["snippet"]["liveBroadcastContent"].ToString() != "none")
                        {
                            info += string.Format(" {0}[{1}]", Color.GREEN, item["snippet"]["liveBroadcastContent"].ToString() == "upcoming" ? "Upcoming Livestream" : "LIVE");
                        }
                        else if (item["contentDetails"]["definition"].ToString() != "hd")
                        {
                            info += string.Format(" {0}[{1}]", Color.RED, item["contentDetails"]["definition"].ToString().ToUpper());
                        }

                        if (item["contentDetails"]["dimension"].ToString() != "2d")
                        {
                            info += string.Format(" {0}[{1}]", Color.RED, item["contentDetails"]["dimension"].ToString().ToUpper());
                        }

                        Bootstrap.Client.Client.Message(e.Recipient,
                            string.Format("{0}» {1}{2}{3}{4} by {5}{6} {7}({8:N0} views, {9:N0} \ud83d\udc4d, {10:N0} \ud83d\udc4e){11}",
                                Color.OLIVE,
                                Color.LIGHTGRAY,
                                item["snippet"]["title"],
                                Color.NORMAL,
                                duration,
                                Color.BLUE,
                                item["snippet"]["channelTitle"],
                                Color.DARKGRAY,
                                int.Parse(item["statistics"]["viewCount"].ToString()),
                                int.Parse(item["statistics"]["likeCount"].ToString()),
                                int.Parse(item["statistics"]["dislikeCount"].ToString()),
                                info
                            )
                        );
                    };

                    webClient.DownloadDataAsync(new Uri(string.Format("https://www.googleapis.com/youtube/v3/videos?part=snippet,contentDetails,statistics&id={0}&key={1}", id, Config.YouTube.ApiKey)));
                }
            }
        }

        private void ProcessTwitch(ChatMessageEventArgs e)
        {
            var matches = TwitchCompiledMatch.Matches(e.Message);

            foreach (Match match in matches)
            {
                var channel = match.Groups["channel"].Value;

                if (LastMatches.Contains(channel))
                {
                    continue;
                }

                LastMatches.Enqueue(channel);

                using (var webClient = new SaneWebClient())
                {
                    webClient.DownloadDataCompleted += (s, twitch) =>
                    {
                        if (twitch.Error != null || twitch.Cancelled)
                        {
                            Log.WriteError("Twitch", "Exception: {0}", twitch.Error.Message);
                            return;
                        }

                        var response = Encoding.UTF8.GetString(twitch.Result);
                        var stream = JsonMapper.ToObject(response);

                        if (stream["stream"] == null)
                        {
                            return;
                        }

                        Bootstrap.Client.Client.Message(e.Recipient,
                            string.Format("{0}» {1}{2} {3}({4:N0} viewers)",
                                Color.OLIVE,
                                Color.LIGHTGRAY,
                                stream["stream"]["channel"]["status"],
                                Color.DARKGRAY,
                                int.Parse(stream["stream"]["viewers"].ToString())
                            )
                        );
                    };
                    
                    webClient.DownloadDataAsync(new Uri(string.Format("https://api.twitch.tv/kraken/streams/{0}", channel)));
                }
            }
        }
    }
}
