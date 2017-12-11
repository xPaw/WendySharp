using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using NetIrc2;
using NetIrc2.Events;
using NetIrc2.Parsing;
using Newtonsoft.Json;

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
                Config = JsonConvert.DeserializeObject<LinkExpanderConfig>(data);

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
                            throw new JsonException($"Invalid channel '{channel}'");
                        }
                    }
                }
            }
            catch (JsonException e)
            {
                Log.WriteError("Twitter", "Failed to parse services.json file: {0}", e.Message);

                Environment.Exit(1);
            }

            LastMatches = new FixedSizedQueue<string>
            {
                Limit = Config.DontRepeatLastCount
            };

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

                if (LastMatches.Contains(e.Recipient + status))
                {
                    continue;
                }

                LastMatches.Enqueue(e.Recipient + status);

                using (var webClient = new SaneWebClient())
                {
                    var url = new UriBuilder("https://api.twitter.com/1.1/statuses/show.json")
                    {
                        Query = $"id={status}&tweet_mode=extended"
                    }.Uri;
                    var authHeader = TwitterAuthorization.GetHeader("GET", url, Config.Twitter);

                    webClient.DownloadDataCompleted += (s, twitter) =>
                    {
                        if (twitter.Error != null || twitter.Cancelled)
                        {
                            Log.WriteError("Twitter", "Exception: {0}", twitter.Error?.Message);
                            return;
                        }

                        var response = Encoding.UTF8.GetString(twitter.Result);
                        dynamic tweet = JsonConvert.DeserializeObject(response);

                        if (tweet.full_text == null)
                        {
                            return;
                        }

                        var text = WebUtility.HtmlDecode(tweet.full_text.ToString()).Replace('\n', ' ').Trim();

                        // Check if original message contains tweet text (with t.co links)
                        if (e.Message.ToString().Contains(text))
                        {
                            return;
                        }

                        if (Config.Twitter.ExpandURLs)
                        {
                            foreach (var entityUrl in tweet.entities.urls)
                            {
                                text = text.Replace(WebUtility.HtmlDecode((string)entityUrl.url), WebUtility.HtmlDecode((string)entityUrl.expanded_url));
                            }

                            if (tweet.entities.media != null)
                            {
                                foreach (var entityUrl in tweet.entities.media)
                                {
                                    text = text.Replace(WebUtility.HtmlDecode((string)entityUrl.url), WebUtility.HtmlDecode((string)entityUrl.expanded_url));
                                }
                            }

                            // Check if original message contains tweet text (with expanded links)
                            if (e.Message.ToString().Contains(text))
                            {
                                return;
                            }
                        }

                        DateTime date;

                        if (!DateTime.TryParseExact(tweet.created_at.ToString(), "ddd MMM dd HH:mm:ss zz00 yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out date))
                        {
                            date = DateTime.UtcNow;
                        }

                        Bootstrap.Client.Client.Message(e.Recipient,
                            $"{Color.OLIVE}» {Color.BLUE}{tweet.user.name}{Color.LIGHTGRAY} {date.ToRelativeString()}{Color.NORMAL}: {text}"
                        );

                        var fakeEvent = new ChatMessageEventArgs(e.Sender, e.Recipient, text);

                        ProcessYoutube(fakeEvent);
                        ProcessTwitch(fakeEvent);
                    };

                    webClient.Headers.Add(HttpRequestHeader.Authorization, $"OAuth {authHeader}");
                    webClient.DownloadDataAsync(url);
                }
            }
        }

        private void ProcessYoutube(ChatMessageEventArgs e)
        {
            var matches = YoutubeCompiledMatch.Matches(e.Message);

            foreach (Match match in matches)
            {
                var id = match.Groups["id"].Value;

                if (LastMatches.Contains(e.Recipient + id))
                {
                    continue;
                }

                LastMatches.Enqueue(e.Recipient + id);

                using (var webClient = new SaneWebClient())
                {
                    webClient.DownloadDataCompleted += (s, youtube) =>
                    {
                        if (youtube.Error != null || youtube.Cancelled)
                        {
                            return;
                        }

                        var response = Encoding.UTF8.GetString(youtube.Result);
                        dynamic data = JsonConvert.DeserializeObject(response);

                        if (data.items == null || data.items.Count == 0)
                        {
                            return;
                        }

                        var item = data.items[0];
                        var info = new List<string>();
                        var time = TimeSpan.Zero;

                        try
                        {
                            time = XmlConvert.ToTimeSpan(item.contentDetails.duration.ToString());
                        }
                        catch (FormatException)
                        {
                            // "longest video on youtube" crashes it due to "W" not being parsed
                        }
                        
                        if (time != TimeSpan.Zero)
                        {
                            info.Add(time.ToString());
                        }

                        if (item.statistics?.viewCount != null)
                        {
                            info.Add($"{Color.DARKGRAY}{int.Parse(item.statistics.viewCount.ToString()):N0}{Color.NORMAL} views");
                        }

                        if (item.statistics?.likeCount != null)
                        {
                            info.Add($"{Color.GREEN}{int.Parse(item.statistics.likeCount.ToString()):N0}{Color.NORMAL} likes");
                            info.Add($"{Color.RED}{int.Parse(item.statistics.dislikeCount.ToString()):N0}{Color.NORMAL} dislikes");
                        }

                        if (item.snippet.liveBroadcastContent?.ToString() != "none")
                        {
                            info.Add(Color.GREEN + item.snippet.liveBroadcastContent.ToString() == "upcoming" ? "Upcoming Livestream" : "LIVE");
                        }
                        else if (item.contentDetails.definition?.ToString() != "hd")
                        {
                            info.Add(Color.RED + item.contentDetails.definition.ToString().ToUpper());
                        }

                        if (item.contentDetails.dimension?.ToString() != "2d")
                        {
                            info.Add(Color.RED + item.contentDetails.dimension.ToString().ToUpper());
                        }

                        Bootstrap.Client.Client.Message(e.Recipient,
                            $"{Color.OLIVE}» {Color.LIGHTGRAY}{item.snippet.title}{Color.NORMAL} by {Color.BLUE}{item.snippet.channelTitle} {Color.NORMAL}({string.Join(", ", info)}{Color.NORMAL})"
                        );
                    };

                    webClient.DownloadDataAsync(new Uri($"https://www.googleapis.com/youtube/v3/videos?part=snippet,contentDetails,statistics&id={id}&key={Config.YouTube.ApiKey}"));
                }
            }
        }

        private void ProcessTwitch(ChatMessageEventArgs e)
        {
            var matches = TwitchCompiledMatch.Matches(e.Message);

            foreach (Match match in matches)
            {
                var channel = match.Groups["channel"].Value;

                if (LastMatches.Contains(e.Recipient + channel))
                {
                    continue;
                }

                LastMatches.Enqueue(e.Recipient + channel);

                using (var webClient = new SaneWebClient())
                {
                    webClient.DownloadDataCompleted += (s, twitch) =>
                    {
                        if (twitch.Error != null || twitch.Cancelled)
                        {
                            Log.WriteError("Twitch", "Exception: {0}", twitch.Error?.Message);
                            return;
                        }

                        var response = Encoding.UTF8.GetString(twitch.Result);
                        dynamic stream = JsonConvert.DeserializeObject(response);

                        if (stream.stream == null)
                        {
                            return;
                        }

                        Bootstrap.Client.Client.Message(e.Recipient,
                            $"{Color.OLIVE}» {Color.LIGHTGRAY}{stream.stream.channel.status} {Color.DARKGRAY}({int.Parse(stream.stream.viewers.ToString()):N0} viewers)"
                        );
                    };
                    
                    webClient.DownloadDataAsync(new Uri($"https://api.twitch.tv/kraken/streams/{channel}?client_id={Config.Twitch.ClientId}"));
                }
            }
        }
    }
}
