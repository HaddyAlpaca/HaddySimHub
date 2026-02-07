using HaddySimHub.Interfaces;

namespace HaddySimHub.Displays.AC;

/// <summary>
/// Provides Assetto Corsa telemetry data from shared memory
/// </summary>
public class ACGameDataProvider : IGameDataProvider<ACTelemetry>
{
    private ACSharedMemoryReader? _reader;
    private readonly Timer? _updateTimer;
    private ACTelemetry _lastTelemetry;

    public event EventHandler<ACTelemetry>? DataReceived;

    public ACGameDataProvider()
    {
        _updateTimer = new Timer(UpdateTelemetry, null, Timeout.Infinite, Timeout.Infinite);
    }

    public void Start()
    {
        _reader = new ACSharedMemoryReader();
        _reader.Connect();

        if (_reader.IsConnected)
        {
            // Update at ~100Hz (10ms intervals)
            _updateTimer?.Change(TimeSpan.Zero, TimeSpan.FromMilliseconds(10));
        }
    }

    public void Stop()
    {
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
