using HaddySimHub.Logging;

namespace HaddySimHub.GameData;

public abstract class GameDataReaderBase(ILogger logger)
{
    protected readonly ILogger _logger = logger;

    public abstract event EventHandler<object> RawDataUpdate;

    public abstract object Convert(object rawData);
}