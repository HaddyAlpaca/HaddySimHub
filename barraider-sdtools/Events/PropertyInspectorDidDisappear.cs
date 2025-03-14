﻿using Newtonsoft.Json;

namespace BarRaider.SdTools.Events
{
    /// <summary>
    /// Payload for PropertyInspectorDidDisappear event
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="action"></param>
    /// <param name="context"></param>
    /// <param name="device"></param>
    public class PropertyInspectorDidDisappear(string action, string context, string device)
    {
        /// <summary>
        /// Action Id
        /// </summary>
        [JsonProperty("action")]
        public string Action { get; private set; } = action;

        /// <summary>
        /// ContextId
        /// </summary>
        [JsonProperty("context")]
        public string Context { get; private set; } = context;

        /// <summary>
        /// Device Guid
        /// </summary>
        [JsonProperty("device")]
        public string Device { get; private set; } = device;
    }
}
