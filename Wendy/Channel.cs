using System;
using System.Collections.Generic;

namespace WendySharp
{
    class Channel
    {
        public string Name { get; private set; }
        public Dictionary<string, byte> Users;
        public bool WeAreOpped;

        public Channel(string name)
        {
            Name = name;
            Users = new Dictionary<string, byte>(StringComparer.InvariantCultureIgnoreCase);
        }

        public void AddUser(string user)
        {
            if (user[0] == '+')
            {
                user = user.Substring(1);
            }
            else if (user[0] == '@')
            {
                user = user.Substring(1);

                if (user.ToLowerInvariant() == Bootstrap.Client.TrueNickname.ToLowerInvariant())
                {
                    WeAreOpped = true;
                }
            }

            if (!HasUser(user))
            {
                Users.Add(user, (byte)1);
            }
        }

        public void RemoveUser(string user)
        {
            if (HasUser(user))
            {
                Users.Remove(user);
            }
        }

        public bool HasUser(string user)
        {
            return Users.ContainsKey(user);
        }
    }
}
