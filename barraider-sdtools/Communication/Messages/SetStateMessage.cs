using Newtonsoft.Json;

namespace BarRaider.SdTools.Communication.Messages
{
    internal class SetStateMessage(uint state, string context) : IMessage
    {
        [JsonProperty("event")]
        public string Event { get { return "setState"; } }

        [JsonProperty("context")]
        public string Context { get; private set; } = context;

        [JsonProperty("payload")]
        public IPayload Payload { get; private set; } = new PayloadClass(state);

        private class PayloadClass(uint state) : IPayload
        {
            [JsonProperty("state")]
            public uint State { get; private set; } = state;
        }
    }
}
