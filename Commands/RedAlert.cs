using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LitJson;
using System.Net;

namespace WendySharp
{
    class RedAlert : Command
    {
        class RedAlertConfig
        {
            public string Channel { get; set; }

            public string AccessToken { get; set; }
        }

        private readonly RedAlertConfig Config;

        public RedAlert()
        {
            Match = new List<string>
            {
                "redalert",
                "topmen",
            };
            Usage = "<text>";
            ArgumentMatch = "(?<text>.+)$";
            HelpText = "Push a bullet to the top men.";
            Permission = "redalert";

            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config", "redalert.json");

            if (!File.Exists(path))
            {
                Log.WriteWarn("Red Alert", "File config/redalert.json doesn't exist");

                return;
            }

            var data = File.ReadAllText(path);

            try
            {
                Config = JsonMapper.ToObject<RedAlertConfig>(data);
            }
            catch (JsonException e)
            {
                Log.WriteError("Red Alert", "Failed to parse redalert.json file: {0}", e.Message);

                Environment.Exit(1);
            }
        }

        public override void OnCommand(CommandArguments command)
        {
            if (string.IsNullOrEmpty(Config.AccessToken))
            {
                command.Reply("Pushbullet is not configured");

                return;
            }

            var text = command.Arguments.Groups["text"].Value.Trim();

            if (text.Length < 3)
            {
                command.Reply("At least 3 characters.");

                return;
            }

            Log.WriteInfo("Red Alert", "'{0}' pushed a bullet: {1}", command.Event.Sender, text);

            using (var webClient = new SaneWebClient())
            {
                webClient.UploadDataCompleted += (s, result) =>
                {
                    if (result.Error != null || result.Cancelled)
                    {
                        command.Reply("Push failed: {0}", result.Error != null ? result.Error.Message : "Timeout");

                        return;
                    }

                    command.Reply("\ud83c\udfc3 Bullets pushed. Top men notified.");
                };
                        
                var data = new JsonData();
                data["type"] = "note";
                data["body"] = text;
                data["title"] = string.Format("{0} in {1}", command.Event.Sender.Nickname, command.Event.Recipient);
                data["channel_tag"] = Config.Channel;

                webClient.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                webClient.Headers.Add(HttpRequestHeader.Authorization, string.Format("Bearer {0}", Config.AccessToken));
                webClient.UploadDataAsync(new Uri("https://api.pushbullet.com/v2/pushes"), "POST", Encoding.UTF8.GetBytes(data.ToJson()));
            }
        }
    }
}
