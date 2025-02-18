using BarRaider.SdTools;

namespace HaddyTruckSDPlugin
{
    internal static class Extensions
    {
        public static string GetPluginId(this object obj)
        {
            // Get the PluginActionId attribute value
            var attribute = (Attribute
                .GetCustomAttributes(obj.GetType(), typeof(PluginActionIdAttribute))
                .FirstOrDefault()) as PluginActionIdAttribute;

            return attribute?.ActionId.Split('.').Last() ?? string.Empty;
        }

    }
}
