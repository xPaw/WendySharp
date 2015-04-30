using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using LitJson;
using NetIrc2.Parsing;

namespace WendySharp
{
    class User
    {
        public string Identity = null; // Assigned from JSON
        public Dictionary<string, List<string>> Permissions; // Assigned from JSON
        public Dictionary<string, Regex> CompiledPermissionsMatch;

        public User()
        {
            Permissions = new Dictionary<string, List<string>>();
            CompiledPermissionsMatch = new Dictionary<string, Regex>();
        }

        public void VerifyAndCompile()
        {
            if (Identity == null)
            {
                throw new JsonException("Missing identity field");
            }

            if (!Permissions.Any())
            {
                throw new JsonException(string.Format("Permission list for '{0}' is empty.", Identity));
            }

            foreach (var channel in Permissions)
            {
                if (channel.Key != "*" && !IrcValidation.IsChannelName(channel.Key))
                {
                    throw new JsonException(string.Format("Invalid channel '{0}' for user '{1}'", channel.Key, Identity));
                }

                if (!channel.Value.Any())
                {
                    throw new JsonException(string.Format("Permission list for '{0}' in channel '{1}' is empty.", Identity, channel.Key));
                }

                string pattern = @"^(" + string.Join("|", channel.Value.Select(x => Regex.Escape(x))).Replace(@"\*", @".*") + @")$";

                Log.WriteDebug("User", "'{0}' permissions in {1}: {2}", Identity, channel.Key, pattern);

                CompiledPermissionsMatch.Add(channel.Key, new Regex(pattern, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture));
            }

            // We no longer need these
            Permissions = null;
        }

        public bool HasPermission(string channel, string permission)
        {
            if (CompiledPermissionsMatch.ContainsKey(channel) && CompiledPermissionsMatch[channel].Match(permission).Success)
            {
                return true;
            }

            if (channel != "*" && HasPermission("*", permission))
            {
                return true;
            }

            return false;
        }
    }
}
