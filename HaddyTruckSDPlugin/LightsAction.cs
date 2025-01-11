using BarRaider.SdTools;

namespace HaddyTruckSDPlugin
{
    [PluginActionId("com.haddyalpaca.truck.lights")]

    public class LightsAction(ISDConnection connection, InitialPayload payload) :
        KeyActionBase(connection, payload)
    {
        protected override string GetActionKeys() => "L";

        protected override string GetStateImage() => 
            _truckData.ParkingLightsOn
                ? (_truckData.LowBeamOn ? "low-beam" : "parking-lights")
                : "off";
    }
}
