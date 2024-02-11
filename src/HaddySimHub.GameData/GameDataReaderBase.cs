namespace HaddySimHub.GameData;

public abstract class GameDataReaderBase
{
    public event EventHandler<object>? RawDataUpdate;

    public event EventHandler<string>? Notification;

    public abstract DisplayType CurrentDisplayType { get; }

    public abstract void Initialize();

    public abstract object Convert(object rawData);

    protected void UpdateRawData(object data) => this.RawDataUpdate?.Invoke(this, data);

    protected void SendNotification(string message) => this.Notification?.Invoke(this, message);
}