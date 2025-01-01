using BarRaider.SdTools;

namespace HaddyTruckSDPlugin;

[PluginActionId("com.haddyalpaca.truck.parking-brake")]
public class ParkingBrakeAction : KeyActionBase
{
    public ParkingBrakeAction(ISDConnection connection, InitialPayload payload) : base(connection, payload)
    {
        this._keyStroke = " ";
    }

    protected override string GetStateImage() =>
        this._truckData.ParkingBrakeOn ? "on" : "off";
}