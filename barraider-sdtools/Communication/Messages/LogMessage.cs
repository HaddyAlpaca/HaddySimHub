using Newtonsoft.Json;

namespace BarRaider.SdTools.Communication.Messages
{
    internal class LogMessage(string message) : IMessage
    {
        [JsonProperty("event")]
        public string Event { get { return "logMessage"; } }

        [JsonProperty("payload")]
        public IPayload Payload { get; private set; } = new PayloadClass(message);

        private class PayloadClass(string message) : IPayload
        {
            [JsonProperty("message")]
            public string Message { get; private set; } = message;
        }
    }
}
