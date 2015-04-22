using System;
using System.Collections.Generic;
using NetIrc2;
using NetIrc2.Events;

namespace WendySharp
{
    class Channels
    {
        private readonly Dictionary<string, Channel> ChannelList;

        public Channels(IrcClient client)
        {
            ChannelList = new Dictionary<string, Channel>();

            client.GotNameListReply += OnNameListReply;
            client.GotJoinChannel += OnJoinChannel;
            client.GotLeaveChannel += OnLeaveChannel;
            client.GotUserKicked += OnUserKicked;
            client.GotUserQuit += OnUserQuit;
            client.GotNameChange += OnNameChange;
        }

        private void OnNameChange(object sender, NameChangeEventArgs e)
        {
            foreach (var channel in ChannelList)
            {
                if (channel.Value.HasUser(e.Identity.Nickname))
                {
                    Log.WriteDebug("chan", "{0} changed their name to {1} in {2}", e.Identity, e.NewName, channel.Key);

                    channel.Value.RemoveUser(e.Identity.Nickname);
                    channel.Value.AddUser(e.NewName);
                }
            }
        }

        private void OnNameListReply(object sender, NameListReplyEventArgs e)
        {
            var names = e.GetNameList();
            var channel = GetChannel(e.Channel);

            foreach (var name in names)
            {
                channel.AddUser(name);
            }
        }

        private void OnJoinChannel(object sender, JoinLeaveEventArgs e)
        {
            var channels = e.GetChannelList();

            foreach (var channel in channels)
            {
                GetChannel(channel).AddUser(e.Identity.Nickname);

                Log.WriteDebug("chan", "{0} joined {1}", e.Identity, channel);
            }
        }

        private void OnLeaveChannel(object sender, JoinLeaveEventArgs e)
        {
            var channels = e.GetChannelList();

            foreach (var channel in channels)
            {
                GetChannel(channel).RemoveUser(e.Identity.Nickname);

                Log.WriteDebug("chan", "{0} left {1}", e.Identity, channel);
            }
        }

        private void OnUserQuit(object sender, QuitEventArgs e)
        {
            foreach (var channel in ChannelList)
            {
                if (channel.Value.HasUser(e.Identity.Nickname))
                {
                    Log.WriteDebug("chan", "{0} quit {1}", e.Identity.Nickname, channel.Key);

                    channel.Value.RemoveUser(e.Identity.Nickname);
                }
            }
        }

        private void OnUserKicked(object sender, KickEventArgs e)
        {
            GetChannel(e.Channel).RemoveUser(e.Recipient);

            Log.WriteDebug("chan", "{0} kicked from {1}", e.Recipient, e.Channel);
        }

        public Channel GetChannel(string channel)
        {
            if (!ChannelList.ContainsKey(channel))
            {
                ChannelList.Add(channel, new Channel(channel));
            }

            return ChannelList[channel];
        }
    }
}
