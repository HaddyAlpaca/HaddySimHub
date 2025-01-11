using BarRaider.SdTools;

namespace HaddyTruckSDPlugin;

[PluginActionId("com.haddyalpaca.truck.parking-brake")]
public class ParkingBrakeAction(ISDConnection connection, InitialPayload payload) :
    KeyActionBase(connection, payload)
{
    protected override string GetActionKeys() => " ";

    protected override string GetStateImage() =>
        this._truckData.ParkingBrakeOn ? "on" : "off";
}