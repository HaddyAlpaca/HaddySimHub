using BarRaider.SdTools;

namespace HaddyTruckSDPlugin;

[PluginActionId("com.haddyalpaca.truck.high-beam")]
public class HighBeamAction(ISDConnection connection, InitialPayload payload) :
    KeyActionBase(connection, payload)
{
    protected override string GetActionKeys() => "K";

    protected override string GetStateImage() =>
        this._truckData.HighBeamOn ? "on" : "off";
}
