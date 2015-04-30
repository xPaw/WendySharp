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

    class SpamConfig
    {
        public string Message { get; set; }
        public uint Duration { get; set; }
        public uint LinesThreshold { get; set; }
        public uint LinesThresholdSeconds { get; set; }
        public uint RepeatThreshold { get; set; }
        public uint RepeatThresholdSeconds { get; set; }

        public readonly FixedSizedQueue<ChatAction> LastActions;

        public SpamConfig()
        {
            LastActions = new FixedSizedQueue<ChatAction>();
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
    }
}
