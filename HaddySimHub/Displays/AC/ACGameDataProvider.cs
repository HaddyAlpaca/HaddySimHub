namespace HaddySimHub.Displays.AC;

/// <summary>
/// Provides Assetto Corsa telemetry data from shared memory
/// </summary>
public class ACGameDataProvider : SharedMemoryGameDataProviderBase<ACSharedMemoryReader, ACTelemetry>
{
    protected override ACSharedMemoryReader CreateReader()
    {
        return new ACSharedMemoryReader();
    }

    protected override void ConnectReader(ACSharedMemoryReader reader)
    {
        reader.Connect();
    }

    protected override void DisconnectReader(ACSharedMemoryReader? reader)
    {
        reader?.Disconnect();
        reader?.Dispose();
    }

    protected override bool IsConnected(ACSharedMemoryReader? reader)
    {
        return reader?.IsConnected ?? false;
    }

    protected override bool TryReadTelemetry(ACSharedMemoryReader reader, out ACTelemetry telemetry)
    {
        return reader.TryReadTelemetry(out telemetry);
    }

    protected override bool HasDataChanged(ACTelemetry current, ACTelemetry last)
    {
        return current.Rpm != last.Rpm;
    }
}
