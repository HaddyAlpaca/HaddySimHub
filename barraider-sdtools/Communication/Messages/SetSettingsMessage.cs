using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BarRaider.SdTools.Communication.Messages
{
    internal class SetSettingsMessage(JObject settings, string context) : IMessage
    {
        [JsonProperty("event")]
        public string Event { get { return "setSettings"; } }

        [JsonProperty("context")]
        public string Context { get; private set; } = context;

        [JsonProperty("payload")]
        public JObject Payload { get; private set; } = settings;
    }
}
