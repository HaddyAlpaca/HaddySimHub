namespace HaddySimHub.Displays.ACRally;

/// <summary>
/// Provides Assetto Corsa Rally telemetry data from shared memory
/// </summary>
public class ACRallyGameDataProvider : SharedMemoryGameDataProviderBase<ACRallySharedMemoryReader, ACRallyTelemetry>
{
    protected override ACRallySharedMemoryReader CreateReader()
    {
        return new ACRallySharedMemoryReader();
    }

    protected override void ConnectReader(ACRallySharedMemoryReader reader)
    {
        reader.Connect();
    }

    protected override void DisconnectReader(ACRallySharedMemoryReader? reader)
    {
        reader?.Disconnect();
        reader?.Dispose();
    }

    protected override bool IsConnected(ACRallySharedMemoryReader? reader)
    {
        return reader?.IsConnected ?? false;
    }

    protected override bool TryReadTelemetry(ACRallySharedMemoryReader reader, out ACRallyTelemetry telemetry)
    {
        return reader.TryReadTelemetry(out telemetry);
    }

    protected override bool HasDataChanged(ACRallyTelemetry current, ACRallyTelemetry last)
    {
        return current.Rpm != last.Rpm;
    }
}
