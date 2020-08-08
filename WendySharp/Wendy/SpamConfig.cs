using System;
using System.Collections.Generic;
using NetIrc2;
using Newtonsoft.Json;

namespace WendySharp
{
    class ChatAction
    {
        public IrcIdentity Identity;
        public DateTime Time;
        public string Message;
    }

    class UserAction
    {
        public DateTime LastPart;
        public byte PartsCount;
        public byte BansCount;

        public UserAction()
        {
            LastPart = DateTime.UtcNow;
            PartsCount = 1;
            BansCount = 0;
        }
    }

    class SpamConfig
    {
        [JsonProperty(Required = Required.Always)]
        public string Message { get; set; }

        [JsonProperty(Required = Required.Always)]
        public uint Duration { get; set; }

        [JsonProperty(Required = Required.Always)]
        public uint LinesThreshold { get; set; }

        [JsonProperty(Required = Required.Always)]
        public uint LinesThresholdSeconds { get; set; }

        [JsonProperty(Required = Required.Always)]
        public uint RepeatThreshold { get; set; }

        [JsonProperty(Required = Required.Always)]
        public uint RepeatThresholdSeconds { get; set; }

        [JsonProperty(Required = Required.Always)]
        public uint QuitsThreshold { get; set; }

        [JsonProperty(Required = Required.Always)]
        public uint QuitsThresholdSeconds { get; set; }

        [JsonProperty(Required = Required.Always)]
        public uint QuitsBanMinutes { get; set; }

        [JsonProperty(Required = Required.Always)]
        public uint UserMentionsInOneMessage { get; set; }

        public readonly FixedSizedQueue<ChatAction> LastActions;
        private readonly Dictionary<string, UserAction> Users;

        public SpamConfig()
        {
            LastActions = new FixedSizedQueue<ChatAction>();
            Users = new Dictionary<string, UserAction>();
        }

        public void AddAction(IrcIdentity sender, string message)
        {
            LastActions.Enqueue(
                new ChatAction
                {
                    Identity = sender,
                    Message = message.ToLowerInvariant(),
                    Time = DateTime.UtcNow,
                }
            );
        }

        public byte AddUserPart(string sender, uint thresholdSeconds)
        {
            if (!Users.ContainsKey(sender))
            {
                Users.Add(sender, new UserAction());

                return 1;
            }

            if (Users[sender].LastPart.AddSeconds(thresholdSeconds) >= DateTime.UtcNow)
            {
                Users[sender].PartsCount++;
            }
            else
            {
                Users[sender].PartsCount = 1;
            }

            Users[sender].LastPart = DateTime.UtcNow;

            return Users[sender].PartsCount;
        }

        public byte AddUserBan(string sender)
        {
            Users[sender].PartsCount = 1;
            return ++Users[sender].BansCount;
        }
    }
}
