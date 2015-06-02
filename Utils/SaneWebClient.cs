using System;
using System.Net;

namespace WendySharp
{
    class SaneWebClient : WebClient
    {
        protected override WebRequest GetWebRequest(Uri address)
        {
            var w = base.GetWebRequest(address) as HttpWebRequest;

            if (w != null)
            {
                var timeout = (int)TimeSpan.FromSeconds(5).TotalMilliseconds;

                w.UserAgent = "Wendy#";
                w.Timeout = timeout;
                w.ReadWriteTimeout = timeout;
            }

            return w;
        }
    }
}
