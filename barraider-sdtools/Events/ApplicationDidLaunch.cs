using BarRaider.SdTools.Payloads;
using Newtonsoft.Json;

namespace BarRaider.SdTools.Events
{
    /// <summary>
    /// Payload for ApplicationDidLaunch event
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="payload"></param>
    public class ApplicationDidLaunch(ApplicationPayload payload)
    {
        /// <summary>
        /// Payload
        /// </summary>
        [JsonProperty("payload")]
        public ApplicationPayload Payload { get; private set; } = payload;
    }
}
