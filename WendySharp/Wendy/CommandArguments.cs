using System;
using NetIrc2.Events;
using System.Text.RegularExpressions;
using NetIrc2.Parsing;

namespace WendySharp
{
    class CommandArguments
    {
        public bool IsDirect;
        public string MatchedCommand;
        public bool AuthorizedWithServices;
        public ChatMessageEventArgs Event;
        public Match Arguments;
        public User User;

        public void ReplyAsNotice(string message, params object[] args)
        {
            Reply(string.Format(message, args), true);
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
                    message = string.Format("{0}: {1}", Event.Sender.Nickname, message);
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
