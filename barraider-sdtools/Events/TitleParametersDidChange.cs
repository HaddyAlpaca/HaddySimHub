using BarRaider.SdTools.Payloads;
using Newtonsoft.Json;

namespace BarRaider.SdTools.Events
{
    /// <summary>
    /// Payload for TitleParametersDidChange event
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="action"></param>
    /// <param name="context"></param>
    /// <param name="device"></param>
    /// <param name="payload"></param>
    public class TitleParametersDidChange(string action, string context, string device, TitleParametersPayload payload)
    {
        /// <summary>
        /// Action Id
        /// </summary>
        [JsonProperty("action")]
        public string Action { get; private set; } = action;

        /// <summary>
        /// Context Id
        /// </summary>
        [JsonProperty("context")]
        public string Context { get; private set; } = context;

        /// <summary>
        /// Device Guid
        /// </summary>
        [JsonProperty("device")]
        public string Device { get; private set; } = device;

        /// <summary>
        /// Payload
        /// </summary>
        [JsonProperty("payload")]
        public TitleParametersPayload Payload { get; private set; } = payload;
    }
}
