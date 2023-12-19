using HaddySimHub.Logging;
using System.Text.Json;

namespace HaddySimHub.GameData;

public abstract class GameDataReaderBase(ILogger logger)
{
    protected readonly ILogger _logger = logger;

    public event EventHandler<object>? RawDataUpdate;

    public abstract object Convert(object rawData);

    protected void UpdateData(object data)
    {
        if (data != null)
        {
            this._logger.Debug($"{JsonSerializer.Serialize(data)}\n");

            this.RawDataUpdate?.Invoke(this, data);
        }
    }
}