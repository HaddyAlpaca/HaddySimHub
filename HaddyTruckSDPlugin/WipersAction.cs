using BarRaider.SdTools;

namespace HaddyTruckSDPlugin;

[PluginActionId("com.haddyalpaca.truck.wipers")]
public class WipersAction(ISDConnection connection, InitialPayload payload) : 
    KeyActionBase(connection, payload)
{
    protected override string GetActionKeys() => "P";

    protected override string GetStateImage() => this._truckData.WipersOn ? "on" : "off";
}