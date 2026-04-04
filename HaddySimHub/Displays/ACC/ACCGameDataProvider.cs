using HaddySimHub;

namespace HaddySimHub.Displays.ACC;

public class ACCGameDataProvider : SharedMemoryGameDataProviderBase<ACCSharedMemoryReader, ACCTelemetry>
{
    private DateTime _lastDataLog = DateTime.MinValue;
    private int _missedReads;
    
    public override void Start()
    {
        Logger.Info("[ACC] Starting game data provider");
        base.Start();

        if (IsConnected(Reader))
        {
            Logger.Info("[ACC] Connected to ACC shared memory, starting data updates (10ms intervals)");
        }
        else
        {
            Logger.Info("[ACC] Not connected to shared memory. Will retry when game is detected...");
        }
    }

    public override void Stop()
    {
        Logger.Info("[ACC] Stopping game data provider");
        base.Stop();
    }

    protected override void UpdateTelemetry(object? state)
    {
        if (Reader == null || !IsConnected(Reader))
        {
            _missedReads++;
            if (_missedReads == 1 || _missedReads % 100 == 0)
            {
                Logger.Debug($"[ACC] UpdateTelemetry: not connected ({_missedReads} consecutive misses)");
            }
            return;
        }

        _missedReads = 0;
        
        if (TryReadTelemetry(Reader, out var telemetry))
        {
            if (HasDataChanged(telemetry, LastTelemetry))
            {
                LastTelemetry = telemetry;
                OnDataReceived(telemetry);
                
                if ((DateTime.Now - _lastDataLog).TotalSeconds > 5)
                {
                    Logger.Debug($"[ACC] Telemetry: RPM={telemetry.Rpms:F0} Speed={telemetry.SpeedKmh:F1} Gear={telemetry.Gear}");
                    _lastDataLog = DateTime.Now;
                }
            }
        }
    }

    protected override ACCSharedMemoryReader CreateReader()
    {
        return new ACCSharedMemoryReader();
    }

    protected override void ConnectReader(ACCSharedMemoryReader reader)
    {
        reader.Connect();
    }

    protected override void DisconnectReader(ACCSharedMemoryReader? reader)
    {
        reader?.Disconnect();
        reader?.Dispose();
    }

    protected override bool IsConnected(ACCSharedMemoryReader? reader)
    {
        return reader?.IsConnected ?? false;
    }

    protected override bool TryReadTelemetry(ACCSharedMemoryReader reader, out ACCTelemetry telemetry)
    {
        return reader.TryReadTelemetry(out telemetry);
    }

    protected override bool HasDataChanged(ACCTelemetry current, ACCTelemetry last)
    {
        return current.Rpms != last.Rpms;
    }
}
