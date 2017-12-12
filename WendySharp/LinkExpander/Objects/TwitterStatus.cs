// The MIT License (MIT)
//
// CoreTweet - A .NET Twitter Library supporting Twitter API 1.1
// Copyright (c) 2013-2017 CoreTweet Development Team
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using Newtonsoft.Json;

namespace WendySharp.Twitter
{
    /// <summary>
    /// Represents the Tweets, which are the basic atomic building block of all things Twitter.
    /// </summary>
    public class TwitterStatus
    {
        /// <summary>
        /// <para>Gets or sets the integer representation of the unique identifier for this Tweet.</para>
        /// <para>See also: https://dev.twitter.com/docs/twitter-ids-json-and-snowflake</para>
        /// </summary>
        [JsonProperty("id")]
        public long Id { get; set; }
        
        /// <summary>
        /// Gets or sets the time when the Tweet was created.
        /// </summary>
        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }
        
        /// <summary>
        /// Gets or sets the array of two unicode code point indices, identifying the inclusive start and exclusive end of the displayable content of the tweet.
        /// </summary>
        [JsonProperty("display_text_range")]
        public int[] DisplayTextRange { get; set; }
        
        /// <summary>
        /// Gets or sets the extended entities which may have multiple entities data.
        /// </summary>
        [JsonProperty("extended_entities")]
        public Entities ExtendedEntities { get; set; }
        
        /// <summary>
        /// <para>Gets or sets a number of approximately how many times the Tweet has been favorited by Twitter users.</para>
        /// <para>Nullable.</para>
        /// </summary>
        [JsonProperty("favorite_count")]
        public int? FavoriteCount { get; set; }
        
        /// <summary>
        /// Gets or sets the entire untruncated Tweet text.
        /// </summary>
        [JsonProperty("full_text")]
        public string FullText { get; set; }

        /// <summary>
        /// <para>Gets or sets the screen name of the original Tweet's author if the represented Tweet is a reply.</para>
        /// <para>Nullable.</para>
        /// </summary>
        [JsonProperty("in_reply_to_screen_name")]
        public string InReplyToScreenName { get; set; }
        
        /// <summary>
        /// <para>Gets or sets a value that determines if the Tweet is a quoted status.</para>
        /// <para>Nullable.</para>
        /// </summary>
        [JsonProperty("is_quoted_status")]
        public bool? IsQuotedStatus { get; set; }
        
        /// <summary>
        /// <para>Gets or sets the integer representation of the unique identifier for the quoted Tweet in the Tweet.</para>
        /// <para>Nullable.</para>
        /// </summary>
        [JsonProperty("quoted_status_id")]
        public long? QuotedStatusId { get; set; }

        /// <summary>
        /// <para>Gets or sets the quoted Tweet in the Tweet.</para>
        /// <para>Nullable.</para>
        /// </summary>
        [JsonProperty("quoted_status")]
        public TwitterStatus QuotedStatus { get; set; }
        
        /// <summary>
        /// Gets or sets a number of approximately how many times the Tweet has been retweeted by Twitter users.
        /// </summary>
        [JsonProperty("retweet_count")]
        public int? RetweetCount { get; set; }

        /// <summary>
        /// <para>Gets or sets a value that determines if the Tweet has been retweeted by the authenticating user.</para>
        /// <para>Nullable.</para>
        /// </summary>
        [JsonProperty("retweeted")]
        public bool? IsRetweeted { get; set; }

        /// <summary>
        /// <para>Gets or sets the original Tweet if the status is a retweet.</para>
        /// <para>Users can amplify the broadcast of tweets authored by other users by retweeting.</para>
        /// <para>Retweets can be distinguished from typical Tweets by the existence of a retweeted_status attribute.</para>
        /// <para>This attribute contains a representation of the original Tweet that was retweeted.</para>
        /// <para>Note that retweets of retweets do not show representations of the intermediary retweet, but only the original tweet.</para>
        /// <para>(Users can also unretweet a retweet they created by deleting their retweet.) </para>
        /// </summary>
        [JsonProperty("retweeted_status")]
        public TwitterStatus RetweetedStatus { get; set; }
        
        /// <summary>
        /// <para>Gets or sets the user who posted the Tweet.</para>
        /// <para>Perspectival attributes embedded within this object are unreliable.</para>
        /// <para>Seealso: https://dev.twitter.com/docs/platform-objects/users</para>
        /// </summary>
        [JsonProperty("user")]
        public User User { get; set; }
    }

    /// <summary>
    /// Represents a user.
    /// </summary>
    public class User
    {
        /// <summary>
        /// <para>Gets or sets the name of the user, as they've defined it.</para>
        /// <para>Not necessarily a person's name.</para>
        /// <para>Typically capped at 20 characters, but subject to be changed.</para>
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    /// <summary>
    /// Represents the metadata and additional contextual information about content posted on Twitter.
    /// </summary>
    public class Entities
    {
        /// <summary>
        /// Gets or sets the hashtags which have been parsed out of the Tweet text.
        /// </summary>
        [JsonProperty("hashtags")]
        public HashtagEntity[] HashTags { get; set; }

        /// <summary>
        /// Gets or sets the media elements uploaded with the Tweet.
        /// </summary>
        [JsonProperty("media")]
        public MediaEntity[] Media { get; set; }

        /// <summary>
        /// Gets or sets the URLs included in the text of a Tweet or within textual fields of a user object.
        /// </summary>
        [JsonProperty("urls")]
        public UrlEntity[] Urls { get; set; }

        /// <summary>
        /// Gets or sets the other Twitter users mentioned in the text of the Tweet.
        /// </summary>
        [JsonProperty("user_mentions")]
        public UserMentionEntity[] UserMentions { get; set; }

        /// <summary>
        /// Gets or sets the symbols which have been parsed out of the Tweet text.
        /// </summary>
        [JsonProperty("symbols")]
        public CashtagEntity[] Symbols { get; set; }
    }

    /// <summary>
    /// Represents an entity object in the content posted on Twitter. This is an <c>abstract</c> class.
    /// </summary>
    public abstract class Entity
    {
        /// <summary>
        /// <para>Gets or sets an array of integers indicating the offsets within the Tweet text where the URL begins and ends.</para>
        /// <para>The first integer represents the location of the first character of the URL in the Tweet text.</para>
        /// <para>The second integer represents the location of the first non-URL character occurring after the URL (or the end of the string if the URL is the last part of the Tweet text).</para>
        /// </summary>
        [JsonProperty("indices")]
        public int[] Indices { get; set; }
    }

    /// <summary>
    /// Represents a symbol object that contains a symbol in the content posted on Twitter.
    /// </summary>
    public abstract class SymbolEntity : Entity
    {
        /// <summary>
        /// Gets or sets the name of the hashtag, minus the leading '#' or '$' character.
        /// </summary>
        [JsonProperty("text")]
        public string Text { get; set; }
    }

    /// <summary>
    /// Represents a &#35;hashtag object.
    /// </summary>
    public class HashtagEntity : SymbolEntity { }

    /// <summary>
    /// Represents a $cashtag object.
    /// </summary>
    public class CashtagEntity : SymbolEntity { }

    /// <summary>
    /// Represents a media object that contains the URLs, sizes and type of the media.
    /// </summary>
    public class MediaEntity : UrlEntity
    {
        /// <summary>
        /// Gets or sets the ID of the media expressed as a 64-bit integer.
        /// </summary>
        [JsonProperty("id")]
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the additional media info provided by the publisher.
        /// </summary>
        [JsonProperty("additional_media_info")]
        public AdditionalMediaInfo AdditionalMediaInfo { get; set; }

        /// <summary>
        /// Gets or sets the alt text.
        /// </summary>
        [JsonProperty("ext_alt_text")]
        public string ExtAltText { get; set; }

        /// <summary>
        /// Gets or sets the URL pointing directly to the uploaded media file.
        /// </summary>
        [JsonProperty("media_url")]
        public string MediaUrl { get; set; }

        /// <summary>
        /// Gets or sets the URL pointing directly to the uploaded media file, for embedding on https pages.
        /// </summary>
        [JsonProperty("media_url_https")]
        public string MediaUrlHttps { get; set; }

        /// <summary>
        /// Gets or sets the object showing available sizes for the media file.
        /// </summary>
        [JsonProperty("sizes")]
        public MediaSizes Sizes { get; set; }

        /// <summary>
        /// <para>Gets or sets the ID  points to the original Tweet.</para>
        /// <para>(Only for Tweets containing media that was originally associated with a different tweet.)</para>
        /// </summary>
        [JsonProperty("source_status_id")]
        public long? SourceStatusId { get; set; }

        /// <summary>
        /// Gets or sets the type of uploaded media.
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the information of the uploaded video or animated GIF.
        /// </summary>
        [JsonProperty("video_info")]
        public VideoInfo VideoInfo { get; set; }
    }

    /// <summary>
    /// Represents the size of the <see cref="MediaSizes"/>.
    /// </summary>
    public class MediaSize
    {
        /// <summary>
        /// Gets or sets the height in pixels of the size.
        /// </summary>
        [JsonProperty("h")]
        public int Height { get; set; }

        /// <summary>
        /// <para>Gets or sets the resizing method used to obtain the size.</para>
        /// <para>A value of fit means that the media was resized to fit one dimension, keeping its native aspect ratio.</para>
        /// <para>A value of crop means that the media was cropped in order to fit a specific resolution.</para>
        /// </summary>
        [JsonProperty("resize")]
        public string Resize { get; set; }

        /// <summary>
        /// Gets or sets the width in pixels of the size.
        /// </summary>
        [JsonProperty("w")]
        public int Width { get; set; }
    }

    /// <summary>
    /// Represents the variations of the media.
    /// </summary>
    public class MediaSizes
    {
        /// <summary>
        /// Gets or sets the information for a large-sized version of the media.
        /// </summary>
        [JsonProperty("large")]
        public MediaSize Large { get; set; }

        /// <summary>
        /// Gets or sets the information for a medium-sized version of the media.
        /// </summary>
        [JsonProperty("medium")]
        public MediaSize Medium { get; set; }

        /// <summary>
        /// Gets or sets the information for a small-sized version of the media.
        /// </summary>
        [JsonProperty("small")]
        public MediaSize Small { get; set; }

        /// <summary>
        /// Gets or sets the information for a thumbnail-sized version of the media.
        /// </summary>
        [JsonProperty("thumb")]
        public MediaSize Thumb { get; set; }
    }

    /// <summary>
    /// Represents a video_info object which is included by a video or animated GIF entity.
    /// </summary>
    public class VideoInfo
    {
        /// <summary>
        /// Gets or sets the aspect ratio of the video,
        /// as a simplified fraction of width and height in a 2-element array.
        /// Typical values are [4, 3] or [16, 9]
        /// </summary>
        [JsonProperty("aspect_ratio")]
        public int[] AspectRatio { get; set; }

        /// <summary>
        /// Gets or sets the length of the video, in milliseconds.
        /// </summary>
        [JsonProperty("duration_millis")]
        public int? DurationMillis { get; set; }

        /// <summary>
        /// Gets or sets the different encodings/streams of the video.
        /// </summary>
        [JsonProperty("variants")]
        public VideoVariant[] Variants { get; set; }
    }

    /// <summary>
    /// Represents a variant of the video.
    /// </summary>
    public class VideoVariant
    {
        /// <summary>
        /// Gets or sets the bitrate of this variant.
        /// </summary>
        [JsonProperty("bitrate")]
        public int? Bitrate { get; set; }

        /// <summary>
        /// Gets or sets the MIME type of this variant.
        /// </summary>
        [JsonProperty("content_type")]
        public string ContentType { get; set; }

        /// <summary>
        /// Gets or sets the URL of the video or playlist.
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; set; }
    }

    public class AdditionalMediaInfo
    {
        [JsonProperty("call_to_actions")]
        public CallToActions CallToActions { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("embeddable")]
        public bool Embeddable { get; set; }

        [JsonProperty("monetizable")]
        public bool Monetizable { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }
    }

    public class CallToActions
    {
        [JsonProperty("watch_now")]
        public MediaAction WatchNow { get; set; }
    }

    public class MediaAction
    {
        /// <summary>
        /// Gets or sets the URL.
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; set; }
    }

    /// <summary>
    /// Represents a URL object that contains the string for display and the raw URL.
    /// </summary>
    public class UrlEntity : Entity
    {
        /// <summary>
        /// Gets or sets the URL to display on clients.
        /// </summary>
        [JsonProperty("display_url")]
        public string DisplayUrl { get; set; }

        /// <summary>
        /// Gets or sets the expanded version of <see cref="DisplayUrl"/>.
        /// </summary>
        // Note that Twitter accepts invalid URLs, for example, "http://..com"
        [JsonProperty("expanded_url")]
        public string ExpandedUrl { get; set; }

        /// <summary>
        /// Gets or sets the wrapped URL, corresponding to the value embedded directly into the raw Tweet text, and the values for the indices parameter.
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; set; }
    }

    /// <summary>
    /// Represents a mention object that contains the user information.
    /// </summary>
    public class UserMentionEntity : Entity
    {
        /// <summary>
        /// Nullable.
        /// Gets or sets the ID of the mentioned user.
        /// </summary>
        [JsonProperty("id")]
        public long? Id { get; set; }

        /// <summary>
        /// Gets or sets display name of the referenced user.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets screen name of the referenced user.
        /// </summary>
        [JsonProperty("screen_name")]
        public string ScreenName { get; set; }

        /// <summary>
        /// Returns the ID of this instance.
        /// </summary>
        /// <returns>The ID of this instance.</returns>
        public override string ToString()
        {
            return this.Id?.ToString("D");
        }
    }
}
