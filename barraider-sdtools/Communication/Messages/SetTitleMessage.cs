using Newtonsoft.Json;

namespace BarRaider.SdTools.Communication.Messages
{
    internal class SetTitleMessage(string title, string context, SDKTarget target, int? state) : IMessage
    {
        [JsonProperty("event")]
        public string Event { get { return "setTitle"; } }

        [JsonProperty("context")]
        public string Context { get; private set; } = context;

        [JsonProperty("payload")]
        public IPayload Payload { get; private set; } = new PayloadClass(title, target, state);

        private class PayloadClass(string title, SDKTarget target, int? state) : IPayload
        {
            [JsonProperty("title")]
            public string Title { get; private set; } = title;

            [JsonProperty("target")]
            public SDKTarget Target { get; private set; } = target;

            [JsonProperty("state", NullValueHandling = NullValueHandling.Ignore)]
            public int? State { get; private set; } = state;
        }
    }
}
