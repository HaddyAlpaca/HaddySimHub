using Newtonsoft.Json;

namespace BarRaider.SdTools.Communication.Messages
{
    internal class RegisterEventMessage(string eventName, string uuid) : IMessage
    {
        [JsonProperty("event")]
        public string Event { get; private set; } = eventName;

        [JsonProperty("uuid")]
        public string UUID { get; private set; } = uuid;
    }
}
