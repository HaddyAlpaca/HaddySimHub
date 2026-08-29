using HaddySimHub.Interfaces;

namespace HaddySimHub.Displays.Msfs;

/// <summary>
/// Polls SimConnect for Microsoft Flight Simulator telemetry.
/// </summary>
/// <remarks>
/// Shaped like <see cref="SharedMemoryGameDataProviderBase{TReader, TTelemetry}"/>
/// -- same ten millisecond timer, same reconnect-while-the-game-is-up behaviour, same
/// "detected but no data" warning that the console dashboard surfaces -- but it cannot
/// derive from it, because that base is tied to <see cref="ISharedMemoryTelemetryReader{T}"/>.
/// <para>
/// There is no equivalent of the shared memory providers' change detection here, and
/// none is needed: those poll a page that may be unchanged, whereas every message
/// SimConnect dispatches is by definition a fresh frame.
/// </para>
/// </remarks>
public sealed class MsfsGameDataProvider : IGameDataProvider<MsfsTelemetry>, IDisposable
{
    private const string ProviderName = "Msfs";

    private readonly ISimConnectClient _client;
    private readonly Timer _timer;

    private int _consecutiveMissedConnections;
    private int _polling;
    private bool _disposed;

    public event EventHandler<MsfsTelemetry>? DataReceived;

    public MsfsGameDataProvider(ISimConnectClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _timer = new Timer(Poll, null, Timeout.Infinite, Timeout.Infinite);
    }

    public void Start()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        _client.Connect();

        if (_client.IsConnected)
        {
            Logger.Info($"[{ProviderName}] Connected to SimConnect, polling for telemetry");
        }
        else
        {
            Logger.Warn($"[{ProviderName}] SimConnect is not available yet - will keep retrying while the game is running");
        }

        // Keep polling even when the connection failed, so a simulator that finishes
        // loading after the display starts is picked up.
        _timer.Change(TimeSpan.Zero, TimeSpan.FromMilliseconds(10));
    }

    public void Stop()
    {
        _timer.Change(Timeout.Infinite, Timeout.Infinite);
        _client.Disconnect();
        _consecutiveMissedConnections = 0;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        Stop();
        _timer.Dispose();
        _client.Dispose();
    }

    private void Poll(object? state)
    {
        // Opening a connection can take longer than the timer interval, and the timer
        // would otherwise start a second overlapping poll.
        if (Interlocked.CompareExchange(ref _polling, 1, 0) != 0)
        {
            return;
        }

        try
        {
            if (!_client.IsConnected)
            {
                _consecutiveMissedConnections++;
                OnMissedConnection(_consecutiveMissedConnections);
                _client.Connect();

                if (!_client.IsConnected)
                {
                    return;
                }

                Logger.Info($"[{ProviderName}] Connected to SimConnect after {_consecutiveMissedConnections} missed attempts");
            }

            _consecutiveMissedConnections = 0;

            if (_client.TryReadTelemetry(out var telemetry))
            {
                DataReceived?.Invoke(this, telemetry);
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"[{ProviderName}] {ex.Message}\n\n{ex.StackTrace}");
        }
        finally
        {
            Interlocked.Exchange(ref _polling, 0);
        }
    }

    /// <summary>
    /// Mirrors the shared memory providers: the first miss is a warning, so a running
    /// game with no telemetry is visible without debug logging, and the rest are debug.
    /// </summary>
    private static void OnMissedConnection(int consecutiveCount)
    {
        if (consecutiveCount == 1)
        {
            Logger.Warn($"[{ProviderName}] Game process detected but SimConnect is not connected - retrying...");
        }
        else if (consecutiveCount % 100 == 0)
        {
            Logger.Debug($"[{ProviderName}] Still not connected to SimConnect ({consecutiveCount} consecutive misses)");
        }
    }
}
