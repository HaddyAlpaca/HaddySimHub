using Newtonsoft.Json;

namespace BarRaider.SdTools
{
    /// <summary>
    /// Class which holds information on the StreamDeck hardware device
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="size"></param>
    /// <param name="type"></param>
    /// <param name="deviceId"></param>
    public class StreamDeckDeviceInfo(StreamDeckDeviceSize size, DeviceType type, string deviceId)
    {
        /// <summary>
        /// Details on number of keys of the StreamDeck hardware device
        /// </summary>
        [JsonProperty(PropertyName = "size")]
        public StreamDeckDeviceSize Size { get; private set; } = size;

        /// <summary>
        /// Type of StreamDeck hardware device
        /// </summary>
        [JsonProperty(PropertyName = "type")]
        public DeviceType Type { get; private set; } = type;

        /// <summary>
        /// Id of the StreamDeck hardware device
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; private set; } = deviceId;

        /// <summary>
        /// Shows class information as string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"Id: {Id} Type: {Type} Size: {Size?.ToString()}";
        }
    }
}
