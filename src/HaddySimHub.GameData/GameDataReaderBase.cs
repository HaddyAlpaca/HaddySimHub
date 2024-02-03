using System.Text.Json;
using HaddySimHub.Logging;

namespace HaddySimHub.GameData;

public abstract class GameDataReaderBase(ILogger logger)
{
#pragma warning disable SA1401 // Fields should be private
    protected readonly ILogger logger = logger;
#pragma warning restore SA1401 // Fields should be private

    private object? lastReceivedData;

    public event EventHandler<object>? RawDataUpdate;

    public event EventHandler<string>? Notification;

    public abstract DisplayType CurrentDisplayType { get; }

    protected object? LastReceivedData { get => this.lastReceivedData; set => this.lastReceivedData = value; }

    public abstract void Initialize();

    public abstract object Convert(object rawData);

    protected void UpdateRawData(object data)
    {
        this.LastReceivedData = data;

        if (data != null)
        {
            this.logger.LogData(data);
            this.RawDataUpdate?.Invoke(this, data);
        }
    }

    protected void SendNotification(string message)
    {
        this.logger.Info($"Send notification: {message}");
        this.Notification?.Invoke(this, message);
    }
}