using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BarRaider.SdTools.Communication.Messages
{
    internal class SetGlobalSettingsMessage(JObject settings, string pluginUUID) : IMessage
    {
        [JsonProperty("event")]
        public string Event { get { return "setGlobalSettings"; } }

        [JsonProperty("context")]
        public string Context { get; private set; } = pluginUUID;

        [JsonProperty("payload")]
        public JObject Payload { get; private set; } = settings;
    }
}
