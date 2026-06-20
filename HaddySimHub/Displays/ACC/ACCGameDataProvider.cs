using HaddySimHub;

namespace HaddySimHub.Displays.ACC;

public class ACCGameDataProvider : SharedMemoryGameDataProviderBase<ACCSharedMemoryReader, ACCTelemetry>
{
    private DateTime _lastDataLog = DateTime.MinValue;

    protected override string ProviderName => "ACC";

    protected override void OnDataChanged(ACCTelemetry telemetry)
    {
        if ((DateTime.Now - _lastDataLog).TotalSeconds > 5)
        {
            Logger.Debug($"[ACC] Telemetry: RPM={telemetry.Rpms:F0} Speed={telemetry.SpeedKmh:F1} Gear={telemetry.Gear}");
            _lastDataLog = DateTime.Now;
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
