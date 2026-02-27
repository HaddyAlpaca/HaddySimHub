namespace HaddySimHub.Displays.ACC;

/// <summary>
/// Provides Assetto Corsa Competizione telemetry data from shared memory
/// </summary>
public class ACCGameDataProvider : SharedMemoryGameDataProviderBase<ACCSharedMemoryReader, ACCTelemetry>
{
    public override void Start()
    {
        Logger.Debug("[ACC] Starting game data provider");
        base.Start();

        Logger.Info($"ACCGameDataProvider: reader connected={IsConnected(Reader)}");
        if (IsConnected(Reader))
        {
            Logger.Info("[ACC] Connected to ACC shared memory, starting data updates");
            Logger.Debug("ACCGameDataProvider: update timer started (10ms intervals).");
        }
        else
        {
            Logger.Debug("ACCGameDataProvider: reader not connected after Connect(). Will retry only when Start() is called again.");
        }
    }

    public override void Stop()
    {
        Logger.Debug("[ACC] Stopping game data provider");
        base.Stop();
    }

    protected override void UpdateTelemetry(object? state)
    {
        if (Reader == null || !IsConnected(Reader))
        {
            Logger.Debug("ACCGameDataProvider: UpdateTelemetry skipped because reader is null or not connected.");
            return;
        }

        if (TryReadTelemetry(Reader, out var telemetry))
        {
            if (HasDataChanged(telemetry, LastTelemetry))
            {
                LastTelemetry = telemetry;
                Logger.Debug($"ACCGameDataProvider: telemetry changed (rpm {telemetry.Rpm}). Raising DataReceived.");
                OnDataReceived(telemetry);
            }
        }
        else
        {
            Logger.Debug("ACCGameDataProvider: TryReadTelemetry returned false.");
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
        return current.Rpm != last.Rpm;
    }
}
