using System;
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

            UsersList.Add(user.Username, user);

            Log.WriteDebug("Users", "Added user '{0}'", user.Username);
        }

        public static bool TryGetUser(string account, out User user)
        {
            return UsersList.TryGetValue(account, out user);
        }
    }
}
