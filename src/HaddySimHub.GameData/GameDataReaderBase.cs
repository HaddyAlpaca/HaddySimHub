using System.Text.Json;
using HaddySimHub.Logging;

namespace HaddySimHub.GameData;

public abstract class GameDataReaderBase(ILogger logger)
{
    private readonly ILogger logger = logger;

    private object? lastReceivedData;

    public event EventHandler<object>? RawDataUpdate;

    public event EventHandler<string>? Notification;

    protected ILogger Logger => this.logger;

    protected object? LastReceivedData { get => this.lastReceivedData; set => this.lastReceivedData = value; }

    public abstract object Convert(object rawData);

    protected void UpdateRawData(object data)
    {
        this.LastReceivedData = data;

        if (data != null)
        {
            this.logger.Debug($"{JsonSerializer.Serialize(data)}\n");

            this.RawDataUpdate?.Invoke(this, data);
        }
    }

    protected void SendNotification(string message)
    {
        this.logger.Info($"Send notification: {message}");
        this.Notification?.Invoke(this, message);
    }
}