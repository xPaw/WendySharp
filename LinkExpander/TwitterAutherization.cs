using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace WendySharp
{
    static class TwitterAuthorization
    {
        public static string GetHeader(string method, Uri uri, LinkExpanderConfig.TwitterConfig Config)
        {
            var nonce = Guid.NewGuid().ToString();
            var timestamp = Convert.ToInt64((DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds).ToString(CultureInfo.InvariantCulture);

            var oauthParameters = new SortedDictionary<string, string>
            {
                { "oauth_consumer_key", Config.ConsumerKey },
                { "oauth_nonce", nonce },
                { "oauth_signature_method", "HMAC-SHA1" },
                { "oauth_timestamp", timestamp },
                { "oauth_token", Config.AccessToken },
                { "oauth_version", "1.0" }
            };

            var signingParameters = new SortedDictionary<string, string>(oauthParameters);

            var parsedQuery = HttpUtility.ParseQueryString(uri.Query);
            foreach (var k in parsedQuery.AllKeys)
            {
                signingParameters.Add(k, parsedQuery[k]);
            }

            var parameterString = string.Join("&",
                signingParameters.Select(k => $"{Uri.EscapeDataString(k.Key)}={Uri.EscapeDataString(k.Value)}")
            );

            var stringToSign = string.Join("&", new[] { method, uri.AbsoluteUri.Replace(uri.Query, string.Empty), parameterString }.Select(Uri.EscapeDataString));
            var signingKey = Config.ConsumerSecret + "&" + Config.AccessSecret;
            var signature = Convert.ToBase64String(new HMACSHA1(Encoding.ASCII.GetBytes(signingKey)).ComputeHash(Encoding.ASCII.GetBytes(stringToSign)));
            
            oauthParameters.Add("oauth_signature", signature);

            var authHeader = string.Join(", ",
                oauthParameters.Select(k => $"{Uri.EscapeDataString(k.Key)}=\"{Uri.EscapeDataString(k.Value)}\"")
            );

            return authHeader;
        }
    }
}
