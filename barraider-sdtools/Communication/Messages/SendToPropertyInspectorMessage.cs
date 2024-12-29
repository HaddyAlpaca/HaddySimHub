using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BarRaider.SdTools.Communication.Messages
{
    internal class SendToPropertyInspectorMessage(string action, JObject data, string context) : IMessage
    {
        [JsonProperty("event")]
        public string Event { get { return "sendToPropertyInspector"; } }

        [JsonProperty("context")]
        public string Context { get; private set; } = context;

        [JsonProperty("payload")]
        public JObject Payload { get; private set; } = data;

        [JsonProperty("action")]
        public string Action { get; private set; } = action;
    }
}
