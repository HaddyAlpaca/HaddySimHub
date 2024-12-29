using BarRaider.SdTools.Payloads;
using Newtonsoft.Json;

namespace BarRaider.SdTools.Events
{
    /// <summary>
    /// Payload for ApplicationDidTerminate event
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="payload"></param>
    public class ApplicationDidTerminate(ApplicationPayload payload)
    {
        /// <summary>
        /// Payload
        /// </summary>
        [JsonProperty("payload")]
        public ApplicationPayload Payload { get; private set; } = payload;
    }
}
