using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using LitJson;
using NetIrc2;
using NetIrc2.Parsing;

namespace WendySharp
{
    class User
    {
        public string Username; // Assigned from JSON
        public Dictionary<string, List<string>> Permissions; // Assigned from JSON
        public Dictionary<string, Regex> CompiledPermissionsMatch;

        public User()
        {
            Username = "Missing username field";
            Permissions = new Dictionary<string, List<string>>();
            CompiledPermissionsMatch = new Dictionary<string, Regex>();
        }

        public void VerifyAndCompile()
        {
            if (!IrcValidation.IsNickname(Username))
            {
                throw new JsonException(string.Format("'{0}' is a invalid username.", Username));
            }

            if (!Permissions.Any())
            {
                throw new JsonException(string.Format("Permission list for '{0}' is empty.", Username));
            }

            foreach (var channel in Permissions)
            {
                if (channel.Key != "*" && !IrcValidation.IsChannelName(channel.Key))
                {
                    throw new JsonException(string.Format("Invalid channel '{0}' for user '{1}'", channel.Key, Username));
                }

                if (!channel.Value.Any())
                {
                    throw new JsonException(string.Format("Permission list for '{0}' in channel '{1}' is empty.", Username, channel.Key));
                }

                string pattern = @"^(" + string.Join("|", channel.Value.Select(x => Regex.Escape(x))).Replace(@"\*", @".*") + @")$";

                Log.WriteDebug("User", "'{0}' permissions in {1}: {2}", Username, channel.Key, pattern);

                CompiledPermissionsMatch.Add(channel.Key, new Regex(pattern, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture));
            }

            // We no longer need these
            Permissions = null;
        }

        public bool HasPermission(string channel, string permission)
        {
            Log.WriteDebug("User", "Checking '{0}' permission for '{1}' in '{2}'", permission, Username, channel);

            if (CompiledPermissionsMatch.ContainsKey(channel) && CompiledPermissionsMatch[channel].Match(permission).Success)
            {
                return true;
            }

            if (channel != "*" && HasPermission("*", permission))
            {
                Log.WriteDebug("User", "Matched wildcard permission for '{0}'", Username);

                return true;
            }

            return false;
        }
    }
}
