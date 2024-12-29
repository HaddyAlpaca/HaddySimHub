using Newtonsoft.Json;
using System;

namespace BarRaider.SdTools.Events
{
    /// <summary>
    /// Payload for DeviceDidDisconnect event
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="device"></param>
    public class DeviceDidDisconnect(String device)
    {
        /// <summary>
        /// Device GUID
        /// </summary>
        [JsonProperty("device")]
        public string Device { get; private set; } = device;
    }
}
