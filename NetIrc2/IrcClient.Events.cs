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
using NetIrc2.Events;

namespace NetIrc2
{
    partial class IrcClient
    {
        /// <summary>
        /// Called when a connection is established.
        /// </summary>
        public event EventHandler Connected;

        /// <summary>
        /// Called when the connection is terminated.
        /// </summary>
        public event EventHandler Closed;

        /// <summary>
        /// Called when the server has begun sending the channel list.
        /// </summary>
        public event EventHandler GotChannelListBegin;

        /// <summary>
        /// Called for each entry of the channel list.
        /// </summary>
        public event EventHandler<ChannelListEntryEventArgs> GotChannelListEntry;

        /// <summary>
        /// Called when the server has finished sending the channel list.
        /// </summary>
        public event EventHandler GotChannelListEnd;

        /// <summary>
        /// Called when a channel's topic changes.
        /// </summary>
        public event EventHandler<ChannelTopicChangeEventArgs> GotChannelTopicChange;

        /// <summary>
        /// Called when someone sends a chat action message.
        /// </summary>
        public event EventHandler<ChatMessageEventArgs> GotChatAction;

        /// <summary>
        /// Called when the client receives an invitation to join a channel.
        /// </summary>
        public event EventHandler<InvitationEventArgs> GotInvitation;

        /// <summary>
        /// Called when an error occurs.
        /// </summary>
        public event EventHandler<IrcErrorEventArgs> GotIrcError;

        /// <summary>
        /// Called when an unknown statement occurs.
        /// </summary>
        public event EventHandler<IrcUnknownStatementEventArgs> GotUnknownIrcStatement;

        /// <summary>
        /// Called when someone joins a channel.
        /// </summary>
        public event EventHandler<JoinLeaveEventArgs> GotJoinChannel;

        /// <summary>
        /// Called when someone leaves a channel.
        /// </summary>
        public event EventHandler<JoinLeaveEventArgs> GotLeaveChannel;

        /// <summary>
        /// Called when someone sends a message.
        /// </summary>
        public event EventHandler<ChatMessageEventArgs> GotMessage;

        /// <summary>
        /// Called when a channel or user's mode is changed.
        /// </summary>
        public event EventHandler<ModeEventArgs> GotMode;

        /// <summary>
        /// Called when the server has begun sending the Message of the Day.
        /// </summary>
        public event EventHandler GotMotdBegin;

        /// <summary>
        /// Called for each line of the Message of the Day sent by the server.
        /// </summary>
        public event EventHandler<SimpleMessageEventArgs> GotMotdText;

        /// <summary>
        /// Called when the server has finished sending the Message of the Day.
        /// </summary>
        public event EventHandler GotMotdEnd;

        /// <summary>
        /// Called when someone changes their name.
        /// </summary>
        public event EventHandler<NameChangeEventArgs> GotNameChange;

        /// <summary>
        /// Called when the server is sending a channel's user list.
        /// </summary>
        public event EventHandler<NameListReplyEventArgs> GotNameListReply;

        /// <summary>
        /// Called at the completion of a channel's user list.
        /// </summary>
        public event EventHandler<NameListEndEventArgs> GotNameListEnd;

        /// <summary>
        /// Called when someone sends a notice. Notices differ from
        /// ordinary messages in that, by convention, one should not
        /// send an automated reply in response (such as 'I am away
        /// from the keyboard.').
        /// </summary>
        public event EventHandler<ChatMessageEventArgs> GotNotice;

        /// <summary>
        /// Called on a user's reply to a ping.
        /// </summary>
        public event EventHandler<PingReplyEventArgs> GotPingReply;

        /// <summary>
        /// Called when a user is kicked from a channel.
        /// </summary>
        public event EventHandler<KickEventArgs> GotUserKicked;

        /// <summary>
        /// Called when a user disconnects from the server.
        /// </summary>
        public event EventHandler<QuitEventArgs> GotUserQuit;

        /// <summary>
        /// Called when the server sends the welcome message.
        /// </summary>
        public event EventHandler<SimpleMessageEventArgs> GotWelcomeMessage;

        protected virtual void OnConnected()
        {
            RaiseConnected();
        }

        protected void RaiseConnected()
        {
            Dispatch(Connected);
        }

        protected virtual void OnClosed()
        {
            RaiseClosed();
        }

        protected void RaiseClosed()
        {
            Dispatch(Closed);
        }

        protected virtual void OnGotChannelListBegin()
        {
            RaiseGotChannelListBegin();
        }

        protected void RaiseGotChannelListBegin()
        {
            Dispatch(GotChannelListBegin);
        }

        protected virtual void OnGotChannelListEntry(ChannelListEntryEventArgs e)
        {
            RaiseGotChannelListEntry(e);
        }

        protected void RaiseGotChannelListEntry(ChannelListEntryEventArgs e)
        {
            Dispatch(GotChannelListEntry, e);
        }

        protected virtual void OnGotChannelListEnd()
        {
            RaiseGotChannelListEnd();
        }

        protected void RaiseGotChannelListEnd()
        {
            Dispatch(GotChannelListEnd);
        }

        protected virtual void OnGotChannelTopicChange(ChannelTopicChangeEventArgs e)
        {
            RaiseGotChannelTopicChange(e);
        }

        protected void RaiseGotChannelTopicChange(ChannelTopicChangeEventArgs e)
        {
            Dispatch(GotChannelTopicChange, e);
        }

        protected virtual void OnGotChatAction(ChatMessageEventArgs e)
        {
            RaiseGotChatAction(e);
        }

        protected void RaiseGotChatAction(ChatMessageEventArgs e)
        {
            Dispatch(GotChatAction, e);
        }

        protected virtual void OnGotInvitation(InvitationEventArgs e)
        {
            RaiseGotInvitation(e);
        }

        protected void RaiseGotInvitation(InvitationEventArgs e)
        {
            Dispatch(GotInvitation, e);
        }

        protected virtual void OnGotIrcError(IrcErrorEventArgs e)
        {
            RaiseGotIrcError(e);
        }

        protected void RaiseGotIrcError(IrcErrorEventArgs e)
        {
            Dispatch(GotIrcError, e);
        }

        protected virtual void OnGotUnknownIrcStatement(IrcUnknownStatementEventArgs e)
        {
            RaiseGotUnknownIrcStatement(e);
        }

        protected void RaiseGotUnknownIrcStatement(IrcUnknownStatementEventArgs e)
        {
            Dispatch(GotUnknownIrcStatement, e);
        }

        protected virtual void OnGotJoinChannel(JoinLeaveEventArgs e)
        {
            RaiseGotJoinChannel(e);
        }

        protected void RaiseGotJoinChannel(JoinLeaveEventArgs e)
        {
            Dispatch(GotJoinChannel, e);
        }

        protected virtual void OnGotLeaveChannel(JoinLeaveEventArgs e)
        {
            RaiseGotLeaveChannel(e);
        }

        protected void RaiseGotLeaveChannel(JoinLeaveEventArgs e)
        {
            Dispatch(GotLeaveChannel, e);
        }

        protected virtual void OnGotMessage(ChatMessageEventArgs e)
        {
            RaiseGotMessage(e);
        }

        protected void RaiseGotMessage(ChatMessageEventArgs e)
        {
            Dispatch(GotMessage, e);
        }

        protected virtual void OnGotMode(ModeEventArgs e)
        {
            RaiseGotMode(e);
        }

        protected void RaiseGotMode(ModeEventArgs e)
        {
            Dispatch(GotMode, e);
        }

        protected virtual void OnGotMotdBegin()
        {
            RaiseGotMotdBegin();
        }

        protected void RaiseGotMotdBegin()
        {
            Dispatch(GotMotdBegin);
        }

        protected virtual void OnGotMotdText(SimpleMessageEventArgs e)
        {
            RaiseGotMotdText(e);
        }

        protected void RaiseGotMotdText(SimpleMessageEventArgs e)
        {
            Dispatch(GotMotdText, e);
        }

        protected virtual void OnGotMotdEnd()
        {
            RaiseGotMotdEnd();
        }

        protected void RaiseGotMotdEnd()
        {
            Dispatch(GotMotdEnd);
        }

        protected virtual void OnGotNameChange(NameChangeEventArgs e)
        {
            RaiseGotNameChange(e);
        }

        protected void RaiseGotNameChange(NameChangeEventArgs e)
        {
            Dispatch(GotNameChange, e);
        }

        protected virtual void OnGotNameListReply(NameListReplyEventArgs e)
        {
            RaiseGotNameListReply(e);
        }

        protected void RaiseGotNameListReply(NameListReplyEventArgs e)
        {
            Dispatch(GotNameListReply, e);
        }

        protected virtual void OnGotNameListEnd(NameListEndEventArgs e)
        {
            RaiseGotNameListEnd(e);
        }

        protected void RaiseGotNameListEnd(NameListEndEventArgs e)
        {
            Dispatch(GotNameListEnd, e);
        }

        protected virtual void OnGotNotice(ChatMessageEventArgs e)
        {
            RaiseGotNotice(e);
        }

        protected void RaiseGotNotice(ChatMessageEventArgs e)
        {
            Dispatch(GotNotice, e);
        }

        protected virtual void OnGotPingReply(PingReplyEventArgs e)
        {
            RaiseGotPingReply(e);
        }

        protected void RaiseGotPingReply(PingReplyEventArgs e)
        {
            Dispatch(GotPingReply, e);
        }

        protected virtual void OnGotUserKicked(KickEventArgs e)
        {
            RaiseGotUserKicked(e);
        }

        protected virtual void OnGotUserQuit(QuitEventArgs e)
        {
            RaiseGotUserQuit(e);
        }

        protected void RaiseGotUserQuit(QuitEventArgs e)
        {
            Dispatch(GotUserQuit, e);
        }

        protected void RaiseGotUserKicked(KickEventArgs e)
        {
            Dispatch(GotUserKicked, e);
        }

        protected virtual void OnGotWelcomeMessage(SimpleMessageEventArgs e)
        {
            RaiseGotWelcomeMessage(e);
        }

        protected void RaiseGotWelcomeMessage(SimpleMessageEventArgs e)
        {
            Dispatch(GotWelcomeMessage, e);
        }

        void Dispatch(EventHandler @event)
        {
            if (@event == null) { return; }
            var sync = _context.SynchronizationContext;
            if (sync == null) { @event(this, EventArgs.Empty); }
            else { sync.Post(_ => @event(this, EventArgs.Empty), null); }
        }

        void Dispatch<T>(EventHandler<T> @event, T e) where T : EventArgs
        {
            if (@event == null) { return; }
            var sync = _context.SynchronizationContext;
            if (sync == null) { @event(this, e); }
            else { sync.Post(_ => @event(this, e), null); }
        }
    }
}
