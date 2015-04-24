#region License
/*
NetIRC2
Copyright (c) 2013 James F. Bellinger <http://www.zer7.com>
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

using System.Linq;
using NetIrc2.Details;

namespace NetIrc2
{
    partial class IrcClient
    {
        /// <summary>
        /// Logs in to the server.
        /// </summary>
        /// <param name="username">A username. If you aren't using a password, this can be anything you want.</param>
        /// <param name="realname">Your real name, or some made up name.</param>
        /// <param name="nickname">The IRC nickname to use.</param>
        /// <param name="hostname">The hostname to send, or <c>null</c> to send a default value.</param>
        /// <param name="servername">The servername to send, or <c>null</c> to send a default value.</param>
        /// <param name="password">The connection password, or <c>null</c> to not use one.</param>
        public void LogIn(IrcString username, IrcString realname, IrcString nickname,
                          IrcString hostname = null, IrcString servername = null,
                          IrcString password = null)
        {
            Throw.If.Null(username, "username").Null(realname, "realname").Null(nickname, "nickname");

            lock (SyncRoot)
            {
                if (password != null)
                {
                    IrcCommand("PASS", password);
                }

                IrcCommand("NICK", nickname);
                IrcCommand("USER", username, hostname ?? "0", servername ?? "*", realname);
            }
        }

        /// <summary>
        /// Changes the channel topic.
        /// </summary>
        /// <param name="channel">The channel whose topic to change.</param>
        /// <param name="newTopic">The new channel topic.</param>
        public void ChangeChannelTopic(IrcString channel, IrcString newTopic)
        {
            Throw.If.Null(channel, "channel").Null(newTopic, "newTopic");

            IrcCommand("TOPIC", channel, newTopic);
        }

        /// <summary>
        /// Changes the client's nickname.
        /// </summary>
        /// <param name="newName">The nickname to change to.</param>
        public void ChangeName(IrcString newName)
        {
            Throw.If.Null(newName, "newName");

            IrcCommand("NICK", newName);
        }

        /// <summary>
        /// Sends an action message to the specified user or channel.
        /// </summary>
        /// <param name="recipient">The user or channel to send the action message to.</param>
        /// <param name="message">The message to send.</param>
        public void ChatAction(IrcString recipient, IrcString message)
        {
            Throw.If.Null(recipient, "recipient").Null(message, "message");

            CtcpCommand(recipient, "ACTION", new[] { message }, false);
        }

        /// <summary>
        /// Invites the specified user to the channel. Channel operator access
        /// may be required.
        /// </summary>
        /// <param name="user">The user to invite.</param>
        /// <param name="channel">The channel to invite the user to.</param>
        public void Invite(IrcString user, IrcString channel)
        {
            Throw.If.Null(user, "user").Null(channel, "channel");

            IrcCommand("INVITE", user, channel);
        }

        /// <summary>
        /// Joins the specified channel.
        /// </summary>
        /// <param name="channel">The channel to join.</param>
        /// <param name="key">The channel key, or <c>null</c> if a key is unnecessary.</param>
        public void Join(IrcString channel, IrcString key = null)
        {
            Throw.If.Null(channel, "channel");

            IrcCommand("JOIN", key != null
                ? new IrcString[2] { channel, key }
                : new IrcString[1] { channel });
        }

        /// <summary>
        /// Kicks the specified user from the channel. Channel operator access may be required.
        /// </summary>
        /// <param name="user">The user to kick.</param>
        /// <param name="channel">The channel to kick the user from.</param>
        /// <param name="reason">The reason the user was kicked, or <c>null</c> to give no reason.</param>
        public void Kick(IrcString user, IrcString channel, IrcString reason)
        {
            Throw.If.Null(user, "user").Null(channel, "channel");

            IrcCommand("KICK", reason != null
                ? new IrcString[3] { channel, user, reason }
                : new IrcString[2] { channel, user });
        }

        /// <summary>
        /// Leaves the specified channel.
        /// </summary>
        /// <param name="channel">The channel to leave.</param>
        public void Leave(IrcString channel)
        {
            Throw.If.Null(channel, "channel");

            IrcCommand("PART", channel);
        }

        /// <summary>
        /// Requests a listing of available channels on the server.
        /// </summary>
        public void ListChannels()
        {
            IrcCommand("LIST");
        }

        /// <summary>
        /// Sends a message to the specified user or channel.
        /// </summary>
        /// <param name="recipient">The user or channel to send the message to.</param>
        /// <param name="message">The message to send.</param>
        public void Message(IrcString recipient, IrcString message)
        {
            Throw.If.Null(recipient, "recipient").Null(message, "message");

            IrcCommand("PRIVMSG", recipient, message);
        }

        /// <summary>
        /// Changes a channel or user's mode.
        /// </summary>
        /// <param name="target">The channel or user to change the mode of.</param>
        /// <param name="command">The mode change, for example +o or +v.</param>
        /// <param name="parameters">The mode change parameters.</param>
        public void Mode(IrcString target, IrcString command, params IrcString[] parameters)
        {
            Throw.If.Null(target, "target").Null(command, "command").NullElements(parameters, "parameters");

            IrcCommand("MODE", new[] { target, command }.Concat(parameters).ToArray());
        }

        /// <summary>
        /// Sends a notice to the specified user.
        /// </summary>
        /// <param name="recipient">The user to send the notice to.</param>
        /// <param name="message">The message to send.</param>
        public void Notice(IrcString recipient, string message)
        {
            Throw.If.Null(recipient, "recipient").Null(message, "message");

            IrcCommand("NOTICE", recipient, message);
        }

        /// <summary>
        /// Pings the specified user.
        /// </summary>
        /// <param name="userToPing">The user to ping.</param>
        public void Ping(IrcString userToPing)
        {
            Throw.If.Null(userToPing, "userToPing");

            CtcpCommand(userToPing, "PING", new IrcString[] { CtcpPingGetTimestamp().ToString() });
        }

        /// <summary>
        /// Logs out from the server.
        /// </summary>
        /// <param name="quitMessage">The quit message, or <c>null</c>.</param>
        public void LogOut(string quitMessage = null)
        {
            IrcCommand("QUIT", quitMessage != null
                ? new IrcString[1] { quitMessage }
                : new IrcString[0]);
        }
    }
}
