using BarRaider.SdTools;

namespace HaddyTruckSDPlugin
{
    [PluginActionId("com.haddyalpaca.truck.confirm-execute")]
    public class ConfirmExecuteAction(ISDConnection connection, InitialPayload payload)
        : KeyActionBase(connection, payload)
    {
        protected override string GetActionKeys() => "{ENTER}";

        protected override string GetStateImage() => "icon";
    }
}
