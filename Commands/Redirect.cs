using System;
using System.Collections.Generic;
using NetIrc2;
using NetIrc2.Parsing;

namespace WendySharp
{
    class Redirect : Command
    {
        private const string TARGETCHANNEL = "##FIX_YOUR_CONNECTION";

        public Redirect()
        {
            Match = new List<string>
            {
                "redirect",
                "fixurshit",
                "fixurconnection",
                "fixyourshit",
                "fixyourconnection",
            };
            Usage = "<nickname> [channel]";
            ArgumentMatch = "(?<nick>[^ ]+)(?: (?<channel>[^ ]+))?";
            HelpText = "Redirects a user to ##FIX_YOUR_CONNECTION or the given channel for a hard-coded length of time (2 hours).";
            Permission = "irc.op.ban";
        }

        public override void OnCommand(CommandArguments command)
        {
            // TODO: If we're not op, we should try to gain op
            var channel = Bootstrap.Client.ChannelList.GetChannel(command.Event.Recipient);

            if (!channel.WeAreOpped)
            {
                command.Reply("I'm not opped, send help.");

                return;
            }

            var nick = command.Arguments.Groups["nick"].Value;
            IrcIdentity ident;

            if (!IrcIdentity.TryParse(nick, out ident))
            {
                command.Reply("Invalid identity.");

                return;
            }
                
            Bootstrap.Client.Whois.Query(ident,
                whoisData =>
                {
                    if (whoisData.Identity.Nickname != null)
                    {
                        ident = whoisData.Identity;
                    }

                    var nickname = ident.Nickname;

                    if (nickname.ToString().ToLowerInvariant() == Bootstrap.Client.TrueNickname.ToLowerInvariant())
                    {
                        Log.WriteInfo("Redirect", "{0} tried to redirect the bot in {1}", command.Event.Sender, command.Event.Recipient);

                        command.Reply("Don't you even dare");

                        return;
                    }

                    if (whoisData.Identity.Nickname != null)
                    {
                        Whois.NormalizeIdentity(ident);
                    }
                    else
                    {
                        if (ident.Username == null)
                        {
                            ident.Username = "*";
                        }

                        if (ident.Hostname == null)
                        {
                            ident.Hostname = "*";
                        }
                    }

                    var targetChannel = command.Arguments.Groups["channel"].Value.Trim();

                    if (targetChannel.Length == 0)
                    {
                        targetChannel = TARGETCHANNEL;
                    }
                    else if (!IrcValidation.IsChannelName(targetChannel))
                    {
                        command.Reply("Invalid target channel.");

                        return;
                    }

                    if (Bootstrap.Client.ModeList.Find(command.Event.Recipient, ident.ToString(), "-b") != null)
                    {
                        command.Reply("{0} is already banned in this channel.", ident);

                        return;
                    }

                    Log.WriteInfo("Redirect", "{0} redirected {1} from {2} to {3}", command.Event.Sender, ident, command.Event.Recipient, targetChannel);

                    var reason = string.Format("Redirected to {0} by {1}", targetChannel, command.Event.Sender.Nickname);

                    Bootstrap.Client.Client.Mode(command.Event.Recipient, "+b", new IrcString[1] { ident + "$" + targetChannel });

                    if (channel.HasUser(nickname))
                    {
                        Bootstrap.Client.Client.Kick(nickname, command.Event.Recipient, reason);
                    }

                    command.ReplyAsNotice("Redirected {0} to {1} for 2 hours", ident, targetChannel);

                    Bootstrap.Client.ModeList.AddLateModeRequest(
                        new LateModeRequest
                        {
                            Channel = command.Event.Recipient,
                            Recipient = ident.ToString(),
                            Mode = "-b",
                            Time = DateTime.UtcNow.AddHours(2),
                            Reason = reason
                        }
                    );
                }
            );
        }
    }
}
