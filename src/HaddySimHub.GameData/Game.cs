using System.Text.Json;
using HaddySimHub.GameData;
using HaddySimHub.Logging;

public abstract class Game
{
    private readonly ILogger logger;
    private bool isRunning = false;

    public Game(IProcessMonitor processMonitor, ILogger logger, CancellationToken cancellationToken)
    {
        this.logger = logger;

        var processTimer = new Timer(
            _ =>
        {
            this.IsRunning = processMonitor.IsRunning(this.ProcessName);

            this.logger.Debug($"Process '{this.ProcessName}' running = {this.isRunning}");
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
                    this.Started?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    this.Stopped?.Invoke(this, EventArgs.Empty);
                }
            }
        }
    }

    protected abstract string ProcessName { get; }

    protected abstract IDisplay CurrentDisplay { get; }

    protected void ProcessData(object data)
    {
        this.logger.Debug($"ProcessData ({this.ProcessName} isRunning = {this.isRunning})");

        if (this.isRunning)
        {
            this.logger.LogData(JsonSerializer.Serialize(data));

            var update = this.CurrentDisplay.GetDisplayUpdate(data);
            this.DisplayUpdate?.Invoke(this, update);
        }
    }

    protected void SendNotification(string message)
    {
        this.Notification?.Invoke(this, message);
    }
}