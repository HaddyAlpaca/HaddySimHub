using Newtonsoft.Json;

namespace BarRaider.SdTools.Events
{
    /// <summary>
    /// Payload for DeviceDidConnect event
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="deviceInfo"></param>
    public class DeviceDidConnect(StreamDeckDeviceInfo deviceInfo)
    {
        /// <summary>
        /// Device GUID
        /// </summary>
        [JsonProperty("device")]
        public string Device { get; private set; } = deviceInfo?.Id;

        /// <summary>
        /// Device Info
        /// </summary>
        [JsonProperty("deviceInfo")]
        public StreamDeckDeviceInfo DeviceInfo { get; private set; } = deviceInfo;
    }
}
