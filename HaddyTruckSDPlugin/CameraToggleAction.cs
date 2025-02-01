using BarRaider.SdTools;

namespace HaddyTruckSDPlugin;

[PluginActionId("com.haddyalpaca.truck.camera-toggle")]
public class CameraToggleAction(ISDConnection connection, InitialPayload payload) :
    KeyActionBase(connection, payload)
{
    private bool _interior = true;

    protected override string GetActionKeys()
    {
        string key = this._interior ? "5" : "1";
        this._interior = !this._interior;
        return key;
    }

    protected override string GetStateImage() => "camera";

    protected override string GetTitle() => this._interior ? "Interior" : "Peek out";
}
