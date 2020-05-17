using System;
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
            TwitterStream.WarningFallingBehindDetected += (_, args) => Log.WriteWarn("Twitter", $"Stream falling behind: {args.WarningMessage.PercentFull} {args.WarningMessage.Code} {args.WarningMessage.Message}");

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

            try
            {
                ProcessTwitter(e);
            }
            catch (Exception ex)
            {
                Log.WriteError(nameof(LinkExpander), $"Exception: {ex}");
            }
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

                var tweet = await TweetAsync.GetTweet(long.Parse(status)).ConfigureAwait(false);

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

            return text.Trim();
        }
    }
}
