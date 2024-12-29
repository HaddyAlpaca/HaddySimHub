using System;

namespace BarRaider.SdTools
{
    /// <summary>
    /// PluginActionId attribute
    /// Used to indicate the UUID in the manifest file that matches to this class
    /// </summary>
    /// <remarks>
    /// Constructor - This attribute is used to indicate the UUID in the manifest file that matches to this class
    /// </remarks>
    /// <param name="ActionId"></param>
    [Serializable]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class PluginActionIdAttribute(string ActionId) : Attribute
    {

        /// <summary>
        /// UUID of the action
        /// </summary>
        public string ActionId { get; private set; } = ActionId;
    }
}
