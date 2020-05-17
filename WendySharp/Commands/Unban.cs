﻿using System;
using System.Collections.Generic;
using NetIrc2;

namespace WendySharp
{
    class Unban : Command
    {
        public Unban()
        {
            Match = new List<string>
            {
                "unban",
                "unquiet",
                "unmute",
                "dequiet",
            };
            Usage = "<nick or hostmask>";
            ArgumentMatch = "(?<nick>[^ ]+)$";
            HelpText = "Un-bans or un-quiets a user.";
            Permission = "irc.op.ban";
        }

        public override void OnCommand(CommandArguments command)
        {
            var nick = command.Arguments.Groups["nick"].Value;

            if (!IrcIdentity.TryParse(nick, out var ident))
            {
                command.Reply("Invalid identity.");

                return;
            }

            var channel = Bootstrap.Client.ChannelList.GetChannel(command.Event.Recipient);

            if (!channel.WeAreOpped)
            {
                if (channel.HasChanServ)
                {
                    Bootstrap.Client.Client.IrcCommand("CHANSERV", "op", channel.Name);
                }
                else
                {
                    command.Reply("I'm not opped, send help.");

                    return;
                }
            }

            var isQuiet = command.MatchedCommand != "unban";

            Bootstrap.Client.Whois.Query(ident,
                whoisData =>
                {
                    if (whoisData.Identity.Nickname != null)
                    {
                        ident = whoisData.Identity;

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

                    Log.WriteInfo("Unban", "'{0}' unbanned '{1}' in {2}", command.Event.Sender, ident, command.Event.Recipient);

                    Bootstrap.Client.Client.Mode(command.Event.Recipient, isQuiet ? "-q" : "-b", ident);

                    command.ReplyAsNotice("{0} {1}", isQuiet ? "Unmuted" : "Unbanned", ident);
                }
            );
        }
    }
}
