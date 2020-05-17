using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NetIrc2.Parsing;
using Newtonsoft.Json;

namespace WendySharp
{
    class User
    {
        [JsonProperty(Required = Required.Always)]
        public string Identity;

        [JsonProperty(Required = Required.Always)]
        public Dictionary<string, List<string>> Permissions;

        private readonly Dictionary<string, Regex> CompiledPermissionsMatch;

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

            if (Permissions.Count == 0)
            {
                throw new JsonException($"Permission list for '{Identity}' is empty.");
            }

            foreach (var channel in Permissions)
            {
                if (channel.Key != "*" && !IrcValidation.IsChannelName(channel.Key))
                {
                    throw new JsonException($"Invalid channel '{channel.Key}' for user '{Identity}'");
                }

                if (channel.Value.Count == 0)
                {
                    throw new JsonException($"Permission list for '{Identity}' in channel '{channel.Key}' is empty.");
                }

                var pattern = "^(" + string.Join("|", channel.Value.Select(Regex.Escape)).Replace(@"\*", ".*") + ")$";

                CompiledPermissionsMatch.Add(channel.Key, new Regex(pattern, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture));
            }
        }

        public bool HasPermission(string channel, string permission)
        {
            if (CompiledPermissionsMatch.ContainsKey(channel) && CompiledPermissionsMatch[channel].Match(permission).Success)
            {
                return true;
            }

            return channel != "*" && HasPermission("*", permission);
        }
    }
}
