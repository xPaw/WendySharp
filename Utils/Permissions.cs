using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace WendySharp
{
    class Permissions
    {
        public Permissions()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config", "users.json");

            if (File.Exists(path))
            {
                var data = File.ReadAllText(path);

                try
                {
                    var users = JsonConvert.DeserializeObject<List<User>>(data);

                    foreach (var user in users)
                    {
                        Users.AddUser(user);
                    }
                }
                catch (JsonException e)
                {
                    Log.WriteError("Permissions", "Failed to parse users.json file: {0}", e.Message);

                    Environment.Exit(1);
                }
            }
            else
            {
                Log.WriteWarn("Permissions", "File config/users.json doesn't exist, no users will have any permissions");
            }
        }
    }
}
