using Newtonsoft.Json;

namespace BarRaider.SdTools.Payloads
{
    /// <summary>
    /// ApplicationPayload
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="application"></param>
    public class ApplicationPayload(string application)
    {
        /// <summary>
        /// Application Name
        /// </summary>
        [JsonProperty("application")]
        public string Application { get; private set; } = application;
    }
}
