using HaddySimHub.Interfaces;

namespace HaddySimHub.Displays;

/// <summary>
/// Base class for game data providers using shared memory polling pattern.
/// Implements the template method pattern for shared timer-based polling.
/// </summary>
/// <typeparam name="TReader">The type of shared memory reader</typeparam>
/// <typeparam name="TTelemetry">The type of telemetry data</typeparam>
public abstract class SharedMemoryGameDataProviderBase<TReader, TTelemetry> : IGameDataProvider<TTelemetry>, IDisposable
    where TReader : class
    where TTelemetry : struct
{
    protected TReader? Reader { get; set; }
    protected Timer? UpdateTimer { get; set; }
    protected TTelemetry LastTelemetry { get; set; }
    private bool _disposed;
    private int _consecutiveMissedConnections;

    public event EventHandler<TTelemetry>? DataReceived;

    protected SharedMemoryGameDataProviderBase()
    {
        UpdateTimer = new Timer(UpdateTelemetry, null, Timeout.Infinite, Timeout.Infinite);
    }

    public virtual void Start()
    {
        ThrowIfDisposed();
        Reader = CreateReader();
        ConnectReader(Reader);

        if (IsConnected(Reader))
        {
            Logger.Info($"[{ProviderName}] Connected to shared memory, polling for telemetry");
        }
        else
        {
            Logger.Warn($"[{ProviderName}] Shared memory not available yet - will keep retrying while the game is running");
        }

        // Keep polling even if the game starts after the app so shared memory can come up later.
        UpdateTimer?.Change(TimeSpan.Zero, TimeSpan.FromMilliseconds(10));
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
    /// A short tag identifying this provider in log output (e.g. "ACC").
    /// Defaults to the type name with any "GameDataProvider" suffix trimmed.
    /// </summary>
    protected virtual string ProviderName =>
        GetType().Name.Replace("GameDataProvider", string.Empty);

    /// <summary>
    /// Called each polling tick when the reader is not connected. Consecutive count starts at 1.
    /// The default logs the first miss at warning level (so a "running but no data" game is
    /// visible without debug logging) and every 100th miss thereafter at debug level.
    /// Override and call <c>base.OnMissedConnection</c> to add metrics.
    /// </summary>
    protected virtual void OnMissedConnection(int consecutiveCount)
    {
        if (consecutiveCount == 1)
        {
            Logger.Warn($"[{ProviderName}] Game process detected but shared memory is not connected - retrying...");
        }
        else if (consecutiveCount % 100 == 0)
        {
            Logger.Debug($"[{ProviderName}] Still not connected to shared memory ({consecutiveCount} consecutive misses)");
        }
    }

    /// <summary>
    /// Called when new telemetry is received and differs from the previous value.
    /// Override to add per-provider logging without duplicating the polling loop.
    /// </summary>
    protected virtual void OnDataChanged(TTelemetry telemetry) { }

    /// <summary>
    /// Called when telemetry update occurs. Can be overridden for logging.
    /// </summary>
    protected virtual void UpdateTelemetry(object? state)
    {
        if (Reader == null)
        {
            return;
        }

        if (!IsConnected(Reader))
        {
            _consecutiveMissedConnections++;
            OnMissedConnection(_consecutiveMissedConnections);
            ConnectReader(Reader);
            if (!IsConnected(Reader))
            {
                return;
            }
        }

        if (_consecutiveMissedConnections > 0)
        {
            Logger.Info($"[{ProviderName}] Reconnected to shared memory after {_consecutiveMissedConnections} missed attempts");
        }

        _consecutiveMissedConnections = 0;

        if (TryReadTelemetry(Reader, out var telemetry))
        {
            if (HasDataChanged(telemetry, LastTelemetry))
            {
                LastTelemetry = telemetry;
                OnDataReceived(telemetry);
                OnDataChanged(telemetry);
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

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        Stop();
        UpdateTimer?.Dispose();
        UpdateTimer = null;
        GC.SuppressFinalize(this);
    }

    protected void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }
}
