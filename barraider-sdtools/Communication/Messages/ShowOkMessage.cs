using Newtonsoft.Json;

namespace BarRaider.SdTools.Communication.Messages
{
    internal class ShowOkMessage(string context) : IMessage
    {
        [JsonProperty("event")]
        public string Event { get { return "showOk"; } }

        [JsonProperty("context")]
        public string Context { get; private set; } = context;
    }
}
