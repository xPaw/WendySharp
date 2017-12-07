using System;
using System.Collections.Generic;
using NetIrc2;

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

        public UserAction()
        {
            LastPart = DateTime.UtcNow;
            PartsCount = 1;
        }
    }

    class SpamConfig
    {
        public string Message { get; set; }
        public uint Duration { get; set; }
        public uint LinesThreshold { get; set; }
        public uint LinesThresholdSeconds { get; set; }
        public uint RepeatThreshold { get; set; }
        public uint RepeatThresholdSeconds { get; set; }
        public uint QuitsThreshold { get; set; }
        public uint QuitsThresholdSeconds { get; set; }
        public uint QuitsBanMinutes { get; set; }

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

        public byte AddUserPart(string sender, string channel, uint thresholdSeconds)
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

        public void ResetUserPart(string sender)
        {
            Users.Remove(sender);
        }
    }
}
