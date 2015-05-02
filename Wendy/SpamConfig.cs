using System;
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
        public uint QuitsThreshold { get; set; }
        public uint QuitsThresholdSeconds { get; set; }
        public uint QuitsBanMinutes { get; set; }

        public readonly FixedSizedQueue<ChatAction> LastActions;
        public readonly FixedSizedQueue<ChatAction> LastQuits;

        public SpamConfig()
        {
            LastActions = new FixedSizedQueue<ChatAction>();
            LastQuits = new FixedSizedQueue<ChatAction>();
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

        public void AddQuit(IrcIdentity sender, string channel)
        {
            LastQuits.Enqueue(
                new ChatAction
                {
                    Identity = sender,
                    Message = channel,
                    Time = DateTime.UtcNow,
                }
            );
        }
    }
}
