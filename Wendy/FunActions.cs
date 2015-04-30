using System;
using System.Text.RegularExpressions;
using NetIrc2;
using NetIrc2.Parsing;

namespace WendySharp
{
    class FunActions
    {
        private readonly Regex MatchNineties;
        private readonly Regex MatchLove;
        private readonly Regex MatchSlap;
        private readonly Regex MatchDeath;
        private readonly Regex MatchChildAbuse;

        public FunActions(BaseClient client)
        {
            MatchNineties = new Regex(
                string.Format("^slaps {0} around a bit", client.Settings.Nickname),
                RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase
            );

            MatchLove = new Regex(
                string.Format("^(hugs|loves|pats|pets) {0}", client.Settings.Nickname),
                RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase
            );

            MatchSlap = new Regex(
                string.Format("^slaps {0}", client.Settings.Nickname),
                RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase
            );

            MatchDeath = new Regex(
                string.Format("^(murders|kills|stabs) {0}", client.Settings.Nickname),
                RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase
            );

            MatchChildAbuse = new Regex(
                string.Format("^(kisses|licks) {0}", client.Settings.Nickname),
                RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase
            );

            client.Client.GotChatAction += OnChatAction;
        }

        private void OnChatAction(object sender, NetIrc2.Events.ChatMessageEventArgs e)
        {
            if (!IrcValidation.IsChannelName(e.Recipient))
            {
                return;
            }

            if (MatchNineties.IsMatch(e.Message))
            {
                Bootstrap.Client.Client.Message(e.Recipient, string.Format("{0}: The 90s called. They want their IRC client back.", e.Sender.Nickname));
            }
            else if (MatchLove.IsMatch(e.Message))
            {
                Bootstrap.Client.Client.ChatAction(e.Recipient, "♥");
            }
            else if (MatchSlap.IsMatch(e.Message))
            {
                Bootstrap.Client.Client.Message(e.Recipient, "I may have deserved that.");
            }
            else if (MatchDeath.IsMatch(e.Message))
            {
                Bootstrap.Client.Client.ChatAction(e.Recipient, "dies. RIP in pepperonis.");
            }
            else if (MatchChildAbuse.IsMatch(e.Message))
            {
                Bootstrap.Client.Client.ChatAction(e.Recipient, "calls the police.");
            }
        }
    }
}
