using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NetIrc2;
using LitJson;
using NetIrc2.Parsing;
using System.Linq;

namespace WendySharp
{
    class User
    {
        // Assigned from JSON
        public string Identity;
        public Dictionary<string, List<string>> Permissions;

        public IrcIdentity IrcIdentity;
        public Dictionary<string, Regex> CompiledPermissionsMatch;

        public User()
        {
            Permissions = new Dictionary<string, List<string>>();
            CompiledPermissionsMatch = new Dictionary<string, Regex>();
        }

        public void VerifyAndCompile()
        {
            if (!IrcIdentity.TryParse(Identity, out IrcIdentity))
            {
                throw new JsonException(string.Format("Failed to parse identity '{0}'.", Identity));
            }

            if (IrcIdentity.Username == null)
            {
                throw new JsonException(string.Format("Identity '{0}' must contain a username.", Identity));
            }

            foreach (var channel in Permissions)
            {
                if (!IrcValidation.IsChannelName(channel.Key))
                {
                    throw new JsonException(string.Format("Invalid channel '{0}' for user '{1}'", channel.Key, Identity));
                }

                string pattern = @"^(" + string.Join("|", channel.Value.Select(x => Regex.Escape(x))).Replace(@"\*", @".*") + @")$";

                Log.WriteDebug("User", "'{0}' permissions in {1}: {2}", IrcIdentity, channel.Key, pattern);

                CompiledPermissionsMatch.Add(channel.Key, new Regex(pattern, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture));
            }

            // We no longer need these
            Identity = null;
            Permissions = null;
        }

        public bool HasPermission(IrcString channel, string permission)
        {
            Log.WriteDebug("User", "Checking '{1}' permission for '{0}'", IrcIdentity.Username, permission);

            return CompiledPermissionsMatch.ContainsKey(channel) && CompiledPermissionsMatch[channel].Match(permission).Success;
        }
    }
}
