using HaddySimHub.Logging;
using System.Text.Json;

namespace HaddySimHub.GameData;

public abstract class GameDataReaderBase(ILogger logger)
{
    protected readonly ILogger _logger = logger;

    protected object? _lastReceivedData;

    public event EventHandler<object>? RawDataUpdate;
    public event EventHandler<string>? Notification;

    public abstract object Convert(object rawData);

    protected void UpdateRawData(object data)
    {
        this._lastReceivedData = data;

        if (data != null)
        {
            this._logger.Debug($"{JsonSerializer.Serialize(data)}\n");

            this.RawDataUpdate?.Invoke(this, data);
        }
    }

    protected void SendNotification(string message)
    {
        this.Notification?.Invoke(this, message);
    }
}