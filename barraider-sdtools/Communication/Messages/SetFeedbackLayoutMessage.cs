using Newtonsoft.Json;

namespace BarRaider.SdTools.Communication.Messages
{
    internal class SetFeedbackLayoutMessage(string layout, string context) : IMessage
    {
        [JsonProperty("event")]
        public string Event { get { return "setFeedbackLayout"; } }

        [JsonProperty("context")]
        public string Context { get; private set; } = context;

        [JsonProperty("payload")]
        public IPayload Payload { get; private set; } = new PayloadClass(layout);

        private class PayloadClass(string layout) : IPayload
        {
            [JsonProperty("layout")]
            public string Layout { get; private set; } = layout;
        }
    }
}
