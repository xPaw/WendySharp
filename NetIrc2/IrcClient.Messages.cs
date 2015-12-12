#region License
/*
NetIRC2
Copyright (c) 2005, 2013 James F. Bellinger <http://www.zer7.com>
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met: 

1. Redistributions of source code must retain the above copyright notice, this
   list of conditions and the following disclaimer. 
2. Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution. 

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
#endregion

using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using NetIrc2.Events;
using NetIrc2.Parsing;

namespace NetIrc2
{
    partial class IrcClient
    {
        protected virtual void OnCtcpCommandReceived(IrcIdentity sender, IrcString recipient, IrcString command,
                                                     IrcString[] parameters, IrcString rawParameter)
        {
            switch ((string)command)
            {
                case "ACTION":
                    OnGotChatAction(new ChatMessageEventArgs(sender, recipient, rawParameter));
                    break;

                case "DCC":
                    if (parameters.Length >= 1)
                    {
                        OnDccCommandReceived(sender, recipient, parameters[0], parameters.Skip(1).ToArray());
                    }
                    break;

                case "PING":
                    if (parameters.Length >= 1 && !IrcValidation.IsChannelName(recipient))
                    {
                        CtcpReply(sender.Nickname, "PING", new IrcString[] { parameters[0] });
                    }
                    break;

                case "TIME":
                    if (!IrcValidation.IsChannelName(recipient))
                    {
                        CtcpReply(sender.Nickname, "TIME", new IrcString[] { CtcpTimeGetNow().ToString
                                  ("ddd MMM dd HH:mm:ss yyyy", DateTimeFormatInfo.InvariantInfo) });
                    }
                    break;

                case "VERSION":
                    if (!IrcValidation.IsChannelName(recipient))
                    {
                        CtcpReply(sender.Nickname, "VERSION", new[] { ClientVersion });
                    }
                    break;
            }
        }

        protected virtual void OnCtcpReplyReceived(IrcIdentity sender, IrcString recipient, IrcString command,
                                                   IrcString[] parameters, IrcString rawParameter)
        {
            switch ((string)command)
            {
                case "PING":
                    if (parameters.Length >= 1)
                    {
                        int timestamp;
                        if (int.TryParse(parameters[0], out timestamp))
                        {
                            int delay = CtcpPingGetTimeDifference(timestamp, CtcpPingGetTimestamp());
                            OnGotPingReply(new PingReplyEventArgs(sender, delay));
                        }
                    }
                    break;
            }
        }

        protected virtual void OnDccCommandReceived(IrcIdentity sender, IrcString recipient,
                                                    IrcString command, IrcString[] parameters)
        {

        }

        protected virtual void OnIrcStatementReceived(IrcStatement statement)
        {
            IrcString ctcpCommand; IrcString[] ctcpParameters; IrcString ctcpRawParameter;

            var source = statement.Source;
            var @params = statement.Parameters;
#if DEBUG
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.Write("> " + (string)new IrcString(statement.ToByteArray()));
            Console.ResetColor();
#endif
            switch ((string)statement.Command)
            {
                case "NICK":
                    if (@params.Count >= 1)
                    {
                        OnGotNameChange(new NameChangeEventArgs(source, @params[0]));
                    }
                    break;

                case "INVITE":
                    if (@params.Count >= 2)
                    {
                        OnGotInvitation(new InvitationEventArgs(source, @params[0], @params[1]));
                    }
                    break;

                case "KICK":
                    if (@params.Count >= 2)
                    {
                        OnGotUserKicked(new KickEventArgs(source,
                                                          @params[1], @params[0],
                                                          @params.Count >= 3 ? @params[2] : null));
                    }
                    break;

                case "PRIVMSG":
                    if (@params.Count >= 2)
                    {
                        if (TryCtcpDecode(@params[1], out ctcpCommand, out ctcpParameters, out ctcpRawParameter))
                        {
                            OnCtcpCommandReceived(source, @params[0], ctcpCommand, ctcpParameters, ctcpRawParameter);
                        }
                        else
                        {
                            OnGotMessage(new ChatMessageEventArgs(source, @params[0], @params[1]));
                        }
                    }
                    break;

                case "NOTICE":
                    if (@params.Count >= 2)
                    {
                        if (TryCtcpDecode(@params[1], out ctcpCommand, out ctcpParameters, out ctcpRawParameter))
                        {
                            OnCtcpReplyReceived(source, @params[0], ctcpCommand, ctcpParameters, ctcpRawParameter);
                        }
                        else
                        {
                            OnGotNotice(new ChatMessageEventArgs(source, @params[0], @params[1]));
                        }
                    }
                    break;

                case "JOIN":
                    if (@params.Count >= 1)
                    {
                        OnGotJoinChannel(new JoinLeaveEventArgs(source, @params[0].Split((byte)',')));
                    }
                    break;

                case "PART":
                    if (@params.Count >= 1)
                    {
                        OnGotLeaveChannel(new JoinLeaveEventArgs(source, @params[0].Split((byte)',')));
                    }
                    break;

                case "QUIT":
                    OnGotUserQuit(new QuitEventArgs(source, @params.Count >= 1 ? @params[0] : null));
                    break;

                case "MODE":
                    if (@params.Count >= 2)
                    {
                        OnGotMode(new ModeEventArgs(source, @params[0], @params[1], @params.Skip(2).ToArray()));
                    }
                    break;

                case "PING":
                    if (@params.Count >= 1)
                    {
                        IrcCommand("PONG", @params[0]);
                    }
                    break;

                case "001":
                    if (@params.Count >= 1)
                    {
                        OnGotWelcomeMessage(new SimpleMessageEventArgs(@params[0]));
                    }
                    break;

                case "375":
                    OnGotMotdBegin();
                    break;

                case "372":
                    if (@params.Count >= 1)
                    {
                        OnGotMotdText(new SimpleMessageEventArgs(@params[@params.Count - 1]));
                    }
                    break;

                case "376":
                    OnGotMotdEnd();
                    break;

                case "353":
                    if (@params.Count >= 2)
                    {
                        OnGotNameListReply(new NameListReplyEventArgs(@params[@params.Count - 2],
                                                                      @params[@params.Count - 1].Split((byte)' ')));
                    }
                    break;

                case "366":
                    if (@params.Count >= 1)
                    {
                        OnGotNameListEnd(new NameListEndEventArgs(@params[0]));
                    }
                    break;

                case "321":
                    OnGotChannelListBegin();
                    break;

                case "322":
                    if (@params.Count >= 4)
                    {
                        int userCount;
                        if (int.TryParse(@params[2], out userCount))
                        {
                            OnGotChannelListEntry(new ChannelListEntryEventArgs(@params[1], userCount, @params[@params.Count - 1]));
                        }
                    }
                    break;

                case "323":
                    OnGotChannelListEnd();
                    break;

                case "331":
                    if (@params.Count >= 3)
                    {
                        OnGotChannelTopicChange(new ChannelTopicChangeEventArgs(@params[1], IrcString.Empty));
                    }
                    break;

                case "332":
                    if (@params.Count >= 3)
                    {
                        OnGotChannelTopicChange(new ChannelTopicChangeEventArgs(@params[1], @params[2]));
                    }
                    break;

                case "TOPIC":
                    if (@params.Count >= 2)
                    {
                        OnGotChannelTopicChange(new ChannelTopicChangeEventArgs(@params[0], @params[1]));
                    }
                    break;

                default:
                    if ((int)statement.ReplyCode >= 400 && (int)statement.ReplyCode < 600)
                    {
                        OnGotIrcError(new Events.IrcErrorEventArgs(statement.ReplyCode, statement));
                    }
                    else
                    {
                        OnGotUnknownIrcStatement(new Events.IrcUnknownStatementEventArgs(statement));
                    }
                    break;
            }
        }
    }
}
