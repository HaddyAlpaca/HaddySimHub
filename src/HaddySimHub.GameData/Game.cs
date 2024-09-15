using System.Text.Json;
using HaddySimHub.GameData;
using HaddySimHub.Logging;

public abstract class Game
{
    private static readonly JsonSerializerOptions serializeOptions = new() { IncludeFields = true };
    private readonly ILogger _logger;

    public Game()
    {
        this._logger = new Logger(this.GetType().Name);
    }

    public event EventHandler<string>? Notification;

    public event EventHandler<DisplayUpdate>? DisplayUpdate;

    public abstract string Description { get; }

    public abstract string ProcessName { get; }

    protected abstract IDisplay CurrentDisplay { get; }

    protected void ProcessData(object data)
    {
        this._logger.LogData(JsonSerializer.Serialize(data, serializeOptions));

        try
        {
            var update = this.CurrentDisplay.GetDisplayUpdate(data);
            this.DisplayUpdate?.Invoke(this, update);
        }
        catch (Exception ex)
        {
            this._logger.Error($"Error processing data: {ex.Message}\n\n{ex.StackTrace}");
        }
    }

    protected void SendNotification(string message)
    {
        this.Notification?.Invoke(this, message);
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