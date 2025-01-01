using BarRaider.SdTools;

namespace HaddyTruckSDPlugin;

[PluginActionId("com.haddyalpaca.truck.high-beam")]
public class HighBeamAction : KeyActionBase
{
    public HighBeamAction(ISDConnection connection, InitialPayload payload) : base(connection, payload)
    {
        this._keyStroke = "K";
    }

    protected override string GetStateImage() =>
        this._truckData.HighBeamOn ? "on" : "off";
}
