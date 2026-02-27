using HaddySimHub.Interfaces;

namespace HaddySimHub.Displays;

/// <summary>
/// Base class for game data providers using shared memory polling pattern.
/// Implements the template method pattern for shared timer-based polling.
/// </summary>
/// <typeparam name="TReader">The type of shared memory reader</typeparam>
/// <typeparam name="TTelemetry">The type of telemetry data</typeparam>
public abstract class SharedMemoryGameDataProviderBase<TReader, TTelemetry> : IGameDataProvider<TTelemetry>
    where TReader : class
    where TTelemetry : struct
{
    protected TReader? Reader { get; set; }
    protected Timer? UpdateTimer { get; set; }
    protected TTelemetry LastTelemetry { get; set; }

    public event EventHandler<TTelemetry>? DataReceived;

    protected SharedMemoryGameDataProviderBase()
    {
        UpdateTimer = new Timer(UpdateTelemetry, null, Timeout.Infinite, Timeout.Infinite);
    }

    public virtual void Start()
    {
        Reader = CreateReader();
        ConnectReader(Reader);

        if (IsConnected(Reader))
        {
            // Update at ~100Hz (10ms intervals)
            UpdateTimer?.Change(TimeSpan.Zero, TimeSpan.FromMilliseconds(10));
        }
    }

    public virtual void Stop()
    {
        UpdateTimer?.Change(Timeout.Infinite, Timeout.Infinite);
        DisconnectReader(Reader);
        Reader = null;
    }

    /// <summary>
    /// Creates a new instance of the reader
    /// </summary>
    protected abstract TReader CreateReader();

    /// <summary>
    /// Connects the reader to the shared memory
    /// </summary>
    protected abstract void ConnectReader(TReader reader);

    /// <summary>
    /// Disconnects and disposes the reader
    /// </summary>
    protected abstract void DisconnectReader(TReader? reader);

    /// <summary>
    /// Checks if the reader is connected
    /// </summary>
    protected abstract bool IsConnected(TReader? reader);

    /// <summary>
    /// Attempts to read telemetry data from the reader
    /// </summary>
    protected abstract bool TryReadTelemetry(TReader reader, out TTelemetry telemetry);

    /// <summary>
    /// Determines if the telemetry data has changed (used for throttling events)
    /// </summary>
    protected abstract bool HasDataChanged(TTelemetry current, TTelemetry last);

    /// <summary>
    /// Called when telemetry update occurs. Can be overridden for logging.
    /// </summary>
    protected virtual void UpdateTelemetry(object? state)
    {
        if (Reader == null || !IsConnected(Reader))
        {
            return;
        }

        if (TryReadTelemetry(Reader, out var telemetry))
        {
            if (HasDataChanged(telemetry, LastTelemetry))
            {
                LastTelemetry = telemetry;
                OnDataReceived(telemetry);
            }
        }
    }

    /// <summary>
    /// Raises the DataReceived event. Protected so derived classes can use it.
    /// </summary>
    protected void OnDataReceived(TTelemetry telemetry)
    {
        DataReceived?.Invoke(this, telemetry);
    }
}
