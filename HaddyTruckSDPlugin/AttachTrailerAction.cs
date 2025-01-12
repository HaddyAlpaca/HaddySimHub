using BarRaider.SdTools;

namespace HaddyTruckSDPlugin
{
    [PluginActionId("com.haddyalpaca.truck.attach-trailer")]

    internal class AttachTrailerAction(ISDConnection connection, InitialPayload payload) :
        KeyActionBase(connection, payload)
    {
        protected override string GetActionKeys() => "T";

        protected override string GetStateImage() =>
            this._truckData.NumberOfTrailersAttached > 0 ? "on" : "off"; 
    }
}
