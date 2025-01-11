using BarRaider.SdTools;

namespace HaddyTruckSDPlugin
{
    [PluginActionId("com.haddyalpaca.truck.lights")]

    public class LightsAction : KeyActionBase
    {
        public LightsAction(ISDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            this._keyStroke = "L";
        }

        protected override string GetStateImage() => 
            _truckData.ParkingLightsOn
                ? (_truckData.LowBeamOn ? "low-beam" : "parking-lights")
                : "off";
    }
}
