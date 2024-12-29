using Newtonsoft.Json;
using System;

namespace BarRaider.SdTools.Communication.Messages
{
    internal class OpenUrlMessage(Uri uri) : IMessage
    {
        [JsonProperty("event")]
        public string Event { get { return "openUrl"; } }

        [JsonProperty("payload")]
        public IPayload Payload { get; private set; } = new PayloadClass(uri);

        private class PayloadClass(Uri uri) : IPayload
        {
            [JsonProperty("url")]
            public string Url { get; private set; } = uri.ToString();
        }
    }
}
