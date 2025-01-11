using BarRaider.SdTools;

namespace HaddyTruckSDPlugin;

[PluginActionId("com.haddyalpaca.truck.hazard-lights")]
public class HazardLightsAction(ISDConnection connection, InitialPayload payload) :
    KeyActionBase(connection, payload)
{
    protected override string GetActionKeys() => "F";

    protected override string GetStateImage() =>
        this._truckData.HazardLightsOn && this._truckData.BlinkerLeftOn ? "on" : "off";
}
