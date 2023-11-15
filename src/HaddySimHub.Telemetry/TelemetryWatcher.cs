namespace HaddySimHub.Telemetry;

public interface ITelemetryWatcher
{
    event EventHandler<TelemetryUpdateEventArgs>? TelemetryUpdated;
    void Start(bool emitRawData, CancellationToken cancellationToken);
}

public class TelemetryWatcher : ITelemetryWatcher
{
    private readonly object lockObject = new();
    private readonly IEnumerable<ITelemetryReader> readers;
    private readonly IProcessMonitor processMonitor;
    private Timer? processTimer;
    private Timer? telemetryTimer;
    private ITelemetryReader? telemetryReader;

    public event EventHandler<TelemetryUpdateEventArgs>? TelemetryUpdated;

    public TelemetryWatcher(
        IEnumerable<ITelemetryReader> readers,
        IProcessMonitor processMonitor)
    {
        this.readers = readers;
        this.processMonitor = processMonitor;
    }

    public void Start(bool emitRawData, CancellationToken cancellationToken)
    {
        //Monitor processes every 10 seconds
        processTimer = new Timer(_ =>
        {
            //Get the process that is running
            ITelemetryReader? currentReader = readers.FirstOrDefault(reader => processMonitor.IsRunning(reader.ProcessName));

            lock (lockObject)
            {
                telemetryReader = currentReader;
            }
        }, cancellationToken, TimeSpan.Zero, TimeSpan.FromSeconds(10));

        //Monitor telemetry at approx. 60Hz
        telemetryTimer = new Timer(_ =>
        {
            //Read the current telemetry
            lock (lockObject)
            {
                if (telemetryReader != null)
                {
                    var data = emitRawData ? telemetryReader.ReadRawData() : telemetryReader.ReadTelemetry();
                    TelemetryUpdated?.Invoke(this, new TelemetryUpdateEventArgs(data));
                }
            }
        }, cancellationToken, TimeSpan.Zero, TimeSpan.FromMilliseconds(16));
    }
}
