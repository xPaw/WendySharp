using System.Collections.Generic;
using NetIrc2;

namespace WendySharp
{
    static class Users
    {
        private static readonly Dictionary<string, User> UsersList = new Dictionary<string, User>();

        public static void AddUser(User user)
        {
            user.VerifyAndCompile();

            UsersList.Add(user.Identity, user);

            Log.WriteInfo("Users", "Added user '{0}'", user.Identity);
        }

        public static bool TryGetUser(IrcIdentity ident, out User user)
        {
            if (Bootstrap.Client.HasIdentifyMsg)
            {
                return UsersList.TryGetValue(ident.Nickname, out user);
            }

            return UsersList.TryGetValue(string.Format("{0}@{1}", ident.Username, ident.Hostname), out user);
        }
    }
}
