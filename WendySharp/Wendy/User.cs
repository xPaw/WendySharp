﻿using System;
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
                throw new JsonException(string.Format("Permission list for '{0}' is empty.", Identity));
            }

            foreach (var channel in Permissions)
            {
                if (channel.Key != "*" && !IrcValidation.IsChannelName(channel.Key))
                {
                    throw new JsonException(string.Format("Invalid channel '{0}' for user '{1}'", channel.Key, Identity));
                }

                if (channel.Value.Count == 0)
                {
                    throw new JsonException(string.Format("Permission list for '{0}' in channel '{1}' is empty.", Identity, channel.Key));
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
