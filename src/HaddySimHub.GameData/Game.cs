using System.Diagnostics;
using System.Text.Json;
using HaddySimHub.GameData;
using HaddySimHub.Logging;

public abstract class Game
{
    private static readonly JsonSerializerOptions serializeOptions = new() { IncludeFields = true };
    protected readonly ILogger _logger;

    public Game()
    {
        this._logger = new Logger(this.GetType().Name);
    }

    public event EventHandler<DisplayUpdate>? DisplayUpdate;

    public abstract string Description { get; }

    public bool IsRunning => this.IsGameRunning();

    protected abstract string _processName { get; }

    protected abstract Func<object, DisplayUpdate> GetDisplayUpdate { get; }

    protected virtual bool IsGameRunning()
    {
        return Process.GetProcessesByName(this._processName).Length != 0;
    }

    protected void ProcessData(object data)
    {
        this._logger.LogData(JsonSerializer.Serialize(data, serializeOptions));

        try
        {
            var update = this.GetDisplayUpdate(data);
            this.DisplayUpdate?.Invoke(this, update);
        }
        catch (Exception ex)
        {
            this._logger.Error($"Error processing data: {ex.Message}\n\n{ex.StackTrace}");
        }
    }

    public virtual void Start()
    {
        this._logger.Info($"Start receiving data from {this.Description}");
    }

    public virtual void Stop()
    {
        this._logger.Info($"Stop receiving data from {this.Description}");
    }
}