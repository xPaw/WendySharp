﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using NetIrc2;
using NetIrc2.Events;
using NetIrc2.Parsing;
using Newtonsoft.Json;
using Tweetinvi;
using Tweetinvi.Events;
using Tweetinvi.Models;
using Tweetinvi.Streaming;

namespace WendySharp
{
    class LinkExpander
    {
        class EntityContainer
        {
            public int Start { get; set; }
            public int End { get; set; }
            public string Replacement { get; set; }
            public string IrcColor { get; set; } = string.Empty;
        }

        private readonly Regex TwitterCompiledMatch;
        private readonly Regex YoutubeCompiledMatch;
        private readonly Regex TwitchCompiledMatch;
        private readonly FixedSizedQueue<string> LastMatches;
        private readonly LinkExpanderConfig Config;
        private readonly Dictionary<long, List<string>> TwitterToChannels;
        public IFilteredStream TwitterStream { get; private set; }

        public LinkExpander(IrcClient client)
        {
            TwitterToChannels = new Dictionary<long, List<string>>();

            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config", "services.json");

            if (!File.Exists(path))
            {
                Log.WriteWarn("Twitter", "File config/services.json doesn't exist");

                return;
            }

            var data = File.ReadAllText(path);

            try
            {
                Config = JsonConvert.DeserializeObject<LinkExpanderConfig>(data, new JsonSerializerSettings { MissingMemberHandling = MissingMemberHandling.Error });

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

            TweetinviConfig.ApplicationSettings.TweetMode = TweetMode.Extended;

            // lul per thread or application credentials
            Auth.ApplicationCredentials = new TwitterCredentials(
                Config.Twitter.ConsumerKey,
                Config.Twitter.ConsumerSecret,
                Config.Twitter.AccessToken,
                Config.Twitter.AccessSecret
            );
            
            if (Config.Twitter.AccountsToFollow.Count > 0)
            {
                var thread = new Thread(StartTwitterStream)
                {
                    Name = "TwitterStream"
                };
                thread.Start();
            }
        }

        private void StartTwitterStream()
        {
            TwitterStream = Tweetinvi.Stream.CreateFilteredStream();
            TwitterStream.MatchingTweetReceived += OnTweetReceived;

            TwitterStream.StallWarnings = true;
            TwitterStream.WarningFallingBehindDetected += (sender, args) =>
            {
                Log.WriteWarn("Twitter", $"Stream falling behind: {args.WarningMessage.PercentFull} {args.WarningMessage.Code} {args.WarningMessage.Message}");
            };

            TwitterStream.StreamStopped += (sender, args) =>
            {
                var ex = args.Exception;
                var twitterDisconnectMessage = args.DisconnectMessage;

                if (ex != null)
                {
                    Log.WriteError("Twitter", ex.ToString());
                }

                if (twitterDisconnectMessage != null)
                {
                    Log.WriteError("Twitter", $"Stream stopped: {twitterDisconnectMessage.Code} {twitterDisconnectMessage.Reason}");
                }

                Task.Factory.StartNew(() =>
                {
                    Thread.Sleep(5000);
                    TwitterStream.StartStreamMatchingAnyConditionAsync();
                });
            };

            var twitterUsers = Tweetinvi.User.GetUsersFromScreenNames(Config.Twitter.AccountsToFollow.Keys);

            foreach (var user in twitterUsers)
            {
                var channels = Config.Twitter.AccountsToFollow.First(u => u.Key.Equals(user.ScreenName, StringComparison.InvariantCultureIgnoreCase));
                
                Log.WriteInfo("Twitter", $"Following @{user.ScreenName}");

                TwitterToChannels.Add(user.Id, channels.Value);

                TwitterStream.AddFollow(user);
            }

            TwitterStream.StartStreamMatchingAnyConditionAsync();
        }

        private void OnTweetReceived(object sender, MatchedTweetReceivedEventArgs matchedTweetReceivedEventArgs)
        {
            // Skip replies
            if (matchedTweetReceivedEventArgs.Tweet.InReplyToUserId != null && !TwitterToChannels.ContainsKey(matchedTweetReceivedEventArgs.Tweet.InReplyToUserId.GetValueOrDefault()))
            {
                Log.WriteDebug("Twitter", $"@{matchedTweetReceivedEventArgs.Tweet.CreatedBy.ScreenName} replied to @{matchedTweetReceivedEventArgs.Tweet.InReplyToScreenName}");
                return;
            }

            if (!TwitterToChannels.ContainsKey(matchedTweetReceivedEventArgs.Tweet.CreatedBy.Id))
            {
                return;
            }

            if (matchedTweetReceivedEventArgs.Tweet.RetweetedTweet != null && TwitterToChannels.ContainsKey(matchedTweetReceivedEventArgs.Tweet.RetweetedTweet.CreatedBy.Id))
            {
                Log.WriteDebug("Twitter", $"@{matchedTweetReceivedEventArgs.Tweet.CreatedBy.ScreenName} retweeted @{matchedTweetReceivedEventArgs.Tweet.RetweetedTweet.CreatedBy.ScreenName}");
                return;
            }

            Log.WriteDebug("Twitter", $"Streamed {matchedTweetReceivedEventArgs.Tweet.Url}: {matchedTweetReceivedEventArgs.Tweet.FullText}");

            var tweet = matchedTweetReceivedEventArgs.Tweet;
            var text = $"{Color.BLUE}@{tweet.CreatedBy.ScreenName}{Color.DARKGRAY} tweeted {tweet.CreatedAt.ToRelativeString()}:{Color.NORMAL} {FormatTweet(tweet)}";

            // Only append tweet url if its not already contained in the tweet (e.g. photo url)
            if (!text.Contains(tweet.Url))
            {
                text += $"{Color.DARKBLUE} {tweet.Url}";
            }

            foreach (var channel in TwitterToChannels[tweet.CreatedBy.Id])
            {
                Bootstrap.Client.Client.Message(channel, text);
            }
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

        private async void ProcessTwitter(ChatMessageEventArgs e)
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

                var tweet = await TweetAsync.GetTweet(long.Parse(status));
                
                if (tweet?.FullText == null)
                {
                    continue;
                }
                
                var text = FormatTweet(tweet);
                var reply = string.Empty;

                // Checking range because some mentions still display it
                if (tweet.SafeDisplayTextRange[0] > 0 && tweet.InReplyToScreenName != null)
                {
                    reply = $" in reply to {Color.BLUE}@{tweet.InReplyToScreenName}{Color.NORMAL}";
                }

                Bootstrap.Client.Client.Message(e.Recipient,
                    $"{Color.OLIVE}» {Color.BLUE}@{tweet.CreatedBy.ScreenName}{Color.LIGHTGRAY} {tweet.CreatedAt.ToRelativeString()}{Color.NORMAL}{reply}: {text}"
                );

                var fakeEvent = new ChatMessageEventArgs(e.Sender, e.Recipient, text);

                ProcessYoutube(fakeEvent);
                ProcessTwitch(fakeEvent);
            }
        }

        private string FormatTweet(ITweet tweet)
        {
            var text = tweet.FullText;

            if (!Config.Twitter.ExpandURLs || tweet.Entities == null)
            {
                return text.Substring(tweet.SafeDisplayTextRange[0]);
            }

            var entities = new List<EntityContainer>();

            if (tweet.Entities.Urls != null)
            {
                foreach (var entity in tweet.Entities.Urls)
                {
                    if (!entities.Exists(x => x.Start == entity.Indices[0]))
                    {
                        entities.Add(new EntityContainer
                        {
                            Start = entity.Indices[0],
                            End = entity.Indices[1],
                            Replacement = entity.ExpandedURL,
                        });
                    }
                }
            }

            if (tweet.Entities.Medias != null)
            {
                foreach (var entity in tweet.Entities.Medias)
                {
                    if (!entities.Exists(x => x.Start == entity.Indices[0]))
                    {
                        entities.Add(new EntityContainer
                        {
                            Start = entity.Indices[0],
                            End = entity.Indices[1],
                            Replacement = entity.ExpandedURL,
                        });
                    }
                }
            }

            if (tweet.Entities.Hashtags != null)
            {
                foreach (var entity in tweet.Entities.Hashtags)
                {
                    if (!entities.Exists(x => x.Start == entity.Indices[0]))
                    {
                        entities.Add(new EntityContainer
                        {
                            Start = entity.Indices[0],
                            End = entity.Indices[1],
                            Replacement = "#" + entity.Text,
                            IrcColor = Color.DARKGRAY,
                        });
                    }
                }
            }

            if (tweet.Entities.UserMentions != null)
            {
                foreach (var entity in tweet.Entities.UserMentions)
                {
                    if (!entities.Exists(x => x.Start == entity.Indices[0]))
                    {
                        entities.Add(new EntityContainer
                        {
                            Start = entity.Indices[0],
                            End = entity.Indices[1],
                            Replacement = "@" + entity.ScreenName,
                            IrcColor = Color.BLUE,
                        });
                    }
                }
            }

            if (tweet.Entities.Symbols != null)
            {
                foreach (var entity in tweet.Entities.Symbols)
                {
                    if (!entities.Exists(x => x.Start == entity.Indices[0]))
                    {
                        entities.Add(new EntityContainer
                        {
                            Start = entity.Indices[0],
                            End = entity.Indices[1],
                            Replacement = "$" + entity.Text,
                            IrcColor = Color.GREEN,
                        });
                    }
                }
            }

            // Ref: https://github.com/twitter/twitter-text/blob/34dc1dd9f10e9171100cdff0cb2b7a9ed9ea2bd6/js/src/lib/convertUnicodeIndices.js
            if (entities.Count > 0)
            {
                entities = entities.OrderBy(e => e.Start).ToList();

                var charIndex = 0;
                var entityIndex = 0;
                var codePointIndex = 0;
                var entityCurrent = entities[0];
                
                while (charIndex < text.Length)
                {
                    if (entityCurrent.Start == codePointIndex)
                    {
                        var len = entityCurrent.End - entityCurrent.Start;
                        entityCurrent.Start = charIndex;
                        entityCurrent.End = charIndex + len;

                        entityIndex++;

                        if (entityIndex == entities.Count)
                        {
                            // no more entity
                            break;
                        }

                        entityCurrent = entities[entityIndex];
                    }

                    if (charIndex < text.Length - 1 && char.IsSurrogatePair(text[charIndex], text[charIndex + 1]))
                    {
                        // Found surrogate pair
                        charIndex++;
                    }

                    codePointIndex++;
                    charIndex++;
                }

                foreach (var entity in entities.OrderByDescending(e => e.Start))
                {
                    if (entity.End < tweet.SafeDisplayTextRange[0])
                    {
                        break;
                    }

                    text = text.Substring(0, entity.Start) + entity.IrcColor + entity.Replacement + Color.NORMAL + text.Substring(entity.End);
                }
            }

            text = text.Substring(tweet.SafeDisplayTextRange[0]);
            text = WebUtility.HtmlDecode(text).Replace('\n', ' ');
            text = text.Trim();

            return text;
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
