using System.Text.Json;
using HaddySimHub.GameData;
using HaddySimHub.Logging;

public abstract class Game
{
    private static readonly JsonSerializerOptions serializeOptions = new() { IncludeFields = true };

    private readonly ILogger _logger;
    private bool isRunning = false;

    public Game(IProcessMonitor processMonitor, CancellationToken cancellationToken)
    {
        this._logger = new Logger(this.GetType().Name);

        var processTimer = new Timer(
            _ =>
        {
            this.IsRunning = processMonitor.IsRunning(this.ProcessName);
        },
            cancellationToken,
            TimeSpan.Zero,
            TimeSpan.FromSeconds(10));
    }

    public event EventHandler? Started;

    public event EventHandler? Stopped;

    public event EventHandler<string>? Notification;

    public event EventHandler<DisplayUpdate>? DisplayUpdate;

    public abstract string Description { get; }

    public bool IsRunning
    {
        get
        {
            return this.isRunning;
        }

        set
        {
            if (this.isRunning != value)
            {
                this.isRunning = value;

                if (value)
                {
                    this._logger.Info($"Process '{this.ProcessName}' started.");
                    this.Started?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    this._logger.Info($"Process '{this.ProcessName}' stopped.");
                    this.Stopped?.Invoke(this, EventArgs.Empty);
                }
            }
        }
    }

    protected abstract string ProcessName { get; }

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
}