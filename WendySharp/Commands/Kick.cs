﻿using System;
using System.Collections.Generic;
using NetIrc2;

namespace WendySharp
{
    class Kick : Command
    {
        public Kick()
        {
            Match = new List<string>
            {
                "kick",
                "gtfo",
                "remove",
                "duckoff"
            };
            Usage = "<nickname> [reason]";
            ArgumentMatch = "(?<nick>[^ ]+)( (?<reason>.*))?$";
            HelpText = "Kicks a user from the current channel.";
            Permission = "irc.op.kick";
        }

        public override void OnCommand(CommandArguments command)
        {
            var nick = command.Arguments.Groups["nick"].Value;

            if (!IrcIdentity.TryParse(nick, out var ident))
            {
                command.Reply("Invalid identity.");

                return;
            }

            if (string.Equals(ident.Nickname.ToString(), Bootstrap.Client.TrueNickname, StringComparison.InvariantCultureIgnoreCase))
            {
                command.Reply("Don't you even dare.");

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

            var reason = command.Arguments.Groups["reason"].Value.Trim();

            if (command.MatchedCommand == "duckoff" && reason.Length == 0)
            {
                reason = "Quack, motherducker";
            }

            Log.WriteInfo("Kick", "'{0}' kicked '{1}' in {2} (reason: {3})", command.Event.Sender, ident, command.Event.Recipient, reason.Length == 0 ? "no reason given" : reason);

            if (command.MatchedCommand == "remove")
            {
                Bootstrap.Client.Client.Remove(ident.Nickname, command.Event.Recipient, reason.Length == 0 ? null : reason);
            }
            else
            {
                Bootstrap.Client.Client.Kick(ident.Nickname, command.Event.Recipient, reason.Length == 0 ? null : reason);
            }

            command.ReplyAsNotice("Kicked {0}", ident);
        }
    }
}
