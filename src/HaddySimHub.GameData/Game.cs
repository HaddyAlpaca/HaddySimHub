using HaddySimHub.GameData;

public sealed class Game
{
    private readonly string description;
    private readonly string processName;
    private bool isRunning = false;

    public Game(string description, string processName, IProcessMonitor processMonitor, CancellationToken cancellationToken)
    {
        this.description = description;
        this.processName = processName;

        var processTimer = new Timer(
            _ =>
        {
            this.IsRunning = processMonitor.IsRunning(this.processName);
        },
            cancellationToken,
            TimeSpan.Zero,
            TimeSpan.FromSeconds(10));
    }

    public event EventHandler? Started;

    public event EventHandler? Stopped;

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

    public string Description => this.description;
}