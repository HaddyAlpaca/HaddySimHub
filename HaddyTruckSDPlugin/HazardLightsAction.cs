using BarRaider.SdTools;

namespace HaddyTruckSDPlugin;

[PluginActionId("com.haddyalpaca.truck.hazard-lights")]
public class HazardLightsAction : KeyActionBase
{
    public HazardLightsAction(ISDConnection connection, InitialPayload payload) : base(connection, payload)
    {
        this._keyStroke = "F";
    }

    protected override string GetStateImage() =>
        this._truckData.HazardLightsOn && this._truckData.BlinkerLeftOn ? "on" : "off";
}
