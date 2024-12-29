using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BarRaider.SdTools.Events
{
    /// <summary>
    /// Payload for SendToPlugin event
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="action"></param>
    /// <param name="context"></param>
    /// <param name="payload"></param>
    public class SendToPlugin(string action, string context, JObject payload)
    {
        /// <summary>
        /// ActionId
        /// </summary>
        [JsonProperty("action")]
        public string Action { get; private set; } = action;

        /// <summary>
        /// ContextId
        /// </summary>
        [JsonProperty("context")]
        public string Context { get; private set; } = context;

        /// <summary>
        /// Payload
        /// </summary>
        [JsonProperty("payload")]
        public JObject Payload { get; private set; } = payload;
    }
}
