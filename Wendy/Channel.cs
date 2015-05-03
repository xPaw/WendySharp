using System;
using System.Collections.Generic;

namespace WendySharp
{
    class Channel
    {
        public const byte Operator = (byte)'@';

        public string Name { get; private set; }
        public string Topic { get; set; }
        public Dictionary<string, byte> Users;

        public bool WeAreOpped
        {
            get
            {
                return Users[Bootstrap.Client.TrueNickname] == Operator;
            }
        }

        public bool HasChanServ
        {
            get
            {
                return Bootstrap.Client.Settings.UsesChanServ && HasUser("ChanServ");
            }
        }

        public Channel(string name)
        {
            Name = name;
            Users = new Dictionary<string, byte>(StringComparer.InvariantCultureIgnoreCase);
        }

        public void AddUser(string user)
        {
            byte status = 0;

            if (user[0] == '+')
            {
                user = user.Substring(1);
            }
            else if (user[0] == '@')
            {
                status = Operator;

                user = user.Substring(user[1] == '+' ? 2 : 1);
            }

            Users[user] = status;
        }

        public void RemoveUser(string user)
        {
            Users.Remove(user);
        }

        public void RenameUser(string oldName, string newName)
        {
            lock (Users)
            {
                if (!HasUser(oldName))
                {
                    return;
                }

                byte status = Users[oldName];

                Users.Remove(oldName);
                Users.Add(newName, status);
            }
        }

        public bool HasUser(string user)
        {
            return Users.ContainsKey(user);
        }
    }
}
