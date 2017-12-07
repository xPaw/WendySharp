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
            client.GotChannelTopicChange += OnChannelTopicChange;
        }

        private void OnChannelTopicChange(object sender, ChannelTopicChangeEventArgs e)
        {
            GetChannel(e.Channel).Topic = e.NewTopic;
        }

        private void OnNameChange(object sender, NameChangeEventArgs e)
        {
            foreach (var channel in ChannelList)
            {
                channel.Value.RenameUser(e.Identity.Nickname, e.NewName);
            }

            if (e.Identity.Nickname == Bootstrap.Client.TrueNickname)
            {
                Bootstrap.Client.TrueNickname = e.NewName;

                Log.WriteInfo("IRC", "Bot's name changed to '{0}'", e.NewName);
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
            }
        }

        private void OnLeaveChannel(object sender, JoinLeaveEventArgs e)
        {
            var channels = e.GetChannelList();

            foreach (var channel in channels)
            {
                GetChannel(channel).RemoveUser(e.Identity.Nickname);
            }
        }

        private void OnUserQuit(object sender, QuitEventArgs e)
        {
            foreach (var channel in ChannelList)
            {
                channel.Value.RemoveUser(e.Identity.Nickname);
            }
        }

        private void OnUserKicked(object sender, KickEventArgs e)
        {
            GetChannel(e.Channel).RemoveUser(e.Recipient);
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
