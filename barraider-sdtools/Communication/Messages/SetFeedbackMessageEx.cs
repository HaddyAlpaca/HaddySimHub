using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BarRaider.SdTools.Communication.Messages
{
    internal class SetFeedbackMessageEx(JObject payload, string pluginUUID) : IMessage
    {
        [JsonProperty("event")]
        public string Event { get { return "setFeedback"; } }

        [JsonProperty("context")]
        public string Context { get; private set; } = pluginUUID;

        [JsonProperty("payload")]
        public JObject Payload { get; private set; } = payload;
    }
}
