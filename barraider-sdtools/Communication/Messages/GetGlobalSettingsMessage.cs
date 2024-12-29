using Newtonsoft.Json;

namespace BarRaider.SdTools.Communication.Messages
{
    internal class GetGlobalSettingsMessage(string pluginUUID) : IMessage
    {
        [JsonProperty("event")]
        public string Event { get { return "getGlobalSettings"; } }

        [JsonProperty("context")]
        public string Context { get; private set; } = pluginUUID;
    }
}
