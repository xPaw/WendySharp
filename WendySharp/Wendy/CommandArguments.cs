﻿using System;
using System.Text.RegularExpressions;
using NetIrc2.Events;

namespace WendySharp
{
    class CommandArguments
    {
        public bool IsDirect;
        public string MatchedCommand;
        public ChatMessageEventArgs Event;
        public Match Arguments;
        public User User;

        public void ReplyAsNotice(string message)
        {
            Reply(message, true);
        }
        
        public void ReplyAsNotice(string message, params object[] args)
        {
            Reply(string.Format(message, args), true);
        }

        public void Reply(string message)
        {
            Reply(message, false);
        }
        
        public void Reply(string message, params object[] args)
        {
            Reply(string.Format(message, args), false);
        }

        private void Reply(string message, bool notice)
        {
            string recipient = Event.Recipient;

            if (recipient[0] == '#')
            {
                if (!notice)
                {
                    message = $"{Event.Sender.Nickname}: {message}";
                }
                else
                {
                    recipient = Event.Sender.Nickname.ToString();
                }
            }
            else
            {
                notice = false;

                recipient = Event.Sender.Nickname.ToString();
            }

            Bootstrap.Client.SendReply(recipient, message, notice);
        }
    }
}
