using HaddySimHub.Interfaces;
using HaddySimHub;

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

        Logger.Info($"ACCGameDataProvider: reader connected={_reader.IsConnected}");

        if (_reader.IsConnected)
        {
            Logger.Info("[ACC] Connected to ACC shared memory, starting data updates");
            // Update at ~100Hz (10ms intervals)
            _updateTimer?.Change(TimeSpan.Zero, TimeSpan.FromMilliseconds(10));
            Logger.Debug("ACCGameDataProvider: update timer started (10ms intervals).");
        }
        else
        {
            Logger.Debug("ACCGameDataProvider: reader not connected after Connect(). Will retry only when Start() is called again.");
        }
        else
        {
            Logger.Error("[ACC] Failed to connect to ACC shared memory");
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
            Logger.Debug("ACCGameDataProvider: UpdateTelemetry skipped because reader is null or not connected.");
            return;
        }

        if (_reader.TryReadTelemetry(out var telemetry))
        {
            // Only fire event if data changed (simple check on RPM)
            if (telemetry.Rpm != _lastTelemetry.Rpm)
            {
                _lastTelemetry = telemetry;
                Logger.Debug($"ACCGameDataProvider: telemetry changed (rpm {telemetry.Rpm}). Raising DataReceived.");
                DataReceived?.Invoke(this, telemetry);
            }
        }
        else
        {
            Logger.Debug("ACCGameDataProvider: TryReadTelemetry returned false.");
        }
    }
}
