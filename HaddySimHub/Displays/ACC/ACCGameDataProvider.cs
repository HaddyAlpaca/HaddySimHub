using HaddySimHub.Interfaces;

namespace HaddySimHub.Displays.ACC;

/// <summary>
/// Provides Assetto Corsa Competizione telemetry data from shared memory
/// </summary>
public class ACCGameDataProvider : IGameDataProvider<ACCTelemetry>
{
    private ACCSharedMemoryReader? _reader;
    private readonly Timer? _updateTimer;
    private ACCTelemetry _lastTelemetry;

    public event EventHandler<ACCTelemetry>? DataReceived;

    public ACCGameDataProvider()
    {
        _updateTimer = new Timer(UpdateTelemetry, null, Timeout.Infinite, Timeout.Infinite);
    }

    public void Start()
    {
        Logger.Debug("[ACC] Starting game data provider");
        _reader = new ACCSharedMemoryReader();
        _reader.Connect();

        if (_reader.IsConnected)
        {
            Logger.Info("[ACC] Connected to ACC shared memory, starting data updates");
            // Update at ~100Hz (10ms intervals)
            _updateTimer?.Change(TimeSpan.Zero, TimeSpan.FromMilliseconds(10));
        }
        else
        {
            Logger.Warn("[ACC] Failed to connect to ACC shared memory");
        }
    }

    public void Stop()
    {
        Logger.Debug("[ACC] Stopping game data provider");
        _updateTimer?.Change(Timeout.Infinite, Timeout.Infinite);
        _reader?.Disconnect();
        _reader?.Dispose();
        _reader = null;
    }

    private void UpdateTelemetry(object? state)
    {
        if (_reader == null || !_reader.IsConnected)
        {
            return;
        }

        if (_reader.TryReadTelemetry(out var telemetry))
        {
            // Only fire event if data changed (simple check on RPM)
            if (telemetry.Rpm != _lastTelemetry.Rpm)
            {
                _lastTelemetry = telemetry;
                DataReceived?.Invoke(this, telemetry);
            }
        }
    }
}
