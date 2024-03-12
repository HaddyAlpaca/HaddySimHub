using HaddySimHub.GameData;

public abstract class Game
{
    private bool isRunning = false;

    public Game(IProcessMonitor processMonitor, CancellationToken cancellationToken)
    {
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
        var update = this.CurrentDisplay.GetDisplayUpdate(data);
        this.DisplayUpdate?.Invoke(this, update);
    }

    protected void SendNotification(string message)
    {
        this.Notification?.Invoke(this, message);
    }
}