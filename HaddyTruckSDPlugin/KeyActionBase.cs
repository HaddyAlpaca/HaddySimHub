using BarRaider.SdTools;
using HaddySimHub.Shared;

namespace HaddyTruckSDPlugin;

public abstract class KeyActionBase : KeypadBase
{
    private readonly SignalRConnection _signalR = new();
    private string? _currentImage = null;
    private string? _currentTitle = null;
    private readonly string _plugInId;

    protected TruckData _truckData = new();

    public KeyActionBase(ISDConnection connection, InitialPayload payload)
        : base(connection, payload)
    {
        this._plugInId = this.GetPluginId();

        this._signalR.DataUpdated += async (s, e) =>
        {
            this._truckData = e;
            await this.UpdateState();
        };
    }

    public override void KeyPressed(KeyPayload payload) { }

    public override void KeyReleased(KeyPayload payload) => Helpers.SendKeys(this.GetActionKeys());

    public override void OnTick()
    {
        try
        {
            this._signalR.StartIfNeeded();
        }
        catch (Exception ex)
        {
            Logger.Instance.LogMessage(TracingLevel.ERROR, ex.Message);
        }
    }

    public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload) { }

    public override void ReceivedSettings(ReceivedSettingsPayload payload) { }

    public override void Dispose()
    {
        try
        {
            this._signalR.Stop();
        }
        catch (Exception ex)
        {
            Logger.Instance.LogMessage(TracingLevel.ERROR, ex.Message);
        }

        GC.SuppressFinalize(this);
    }

    private async Task UpdateState()
    {
        string image = this.GetStateImage();
        if (this._currentImage != image)
        {
            await Connection.SetImageAsync(Path.Combine("images", this._plugInId, image));
            this._currentImage = image;
        }

        string title = this.GetTitle();
        if (this._currentTitle != title)
        {
            await Connection.SetTitleAsync(title);
            this._currentTitle = title;
        }
    }

    protected abstract string GetActionKeys();

    protected abstract string GetStateImage();

    protected virtual string GetTitle() => string.Empty;
}
