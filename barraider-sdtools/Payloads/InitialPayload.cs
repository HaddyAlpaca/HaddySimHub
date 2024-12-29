using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using BarRaider.SdTools.Payloads;

namespace BarRaider.SdTools
{
    /// <summary>
    /// Payload received during the plugin's constructor
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="appearancePayload"></param>
    /// <param name="deviceInfo"></param>
    public class InitialPayload(AppearancePayload appearancePayload, StreamDeckInfo deviceInfo)
    {
        /// <summary>
        /// Plugin instance's settings (set through Property Inspector)
        /// </summary>
        [JsonProperty("settings")]
        public JObject Settings { get; private set; } = appearancePayload.Settings;

        /// <summary>
        /// Plugin's physical location on the Stream Deck device
        /// </summary>
        [JsonProperty("coordinates")]
        public KeyCoordinates Coordinates { get; private set; } = appearancePayload.Coordinates;

        /// <summary>
        /// Current plugin state
        /// </summary>
        [JsonProperty("state")]
        public uint State { get; private set; } = appearancePayload.State;

        /// <summary>
        /// Is it in a Multiaction
        /// </summary>
        [JsonProperty("isInMultiAction")]
        public bool IsInMultiAction { get; private set; } = appearancePayload.IsInMultiAction;

        /// <summary>
        /// The controller of the current action. Values include "Keypad" and "Encoder".
        /// </summary>
        [JsonProperty("controller")]
        public string Controller { get; private set; } = appearancePayload.Controller;

        /// <summary>
        /// Information regarding the Stream Deck hardware device
        /// </summary>
        [JsonProperty("deviceInfo", Required = Required.AllowNull)]
        public StreamDeckInfo DeviceInfo { get; private set; } = deviceInfo;
    }
}
