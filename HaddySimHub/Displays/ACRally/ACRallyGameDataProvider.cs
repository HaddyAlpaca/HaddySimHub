namespace HaddySimHub.Displays.ACRally;

/// <summary>
/// Provides Assetto Corsa Rally telemetry from shared memory.
/// </summary>
public class ACRallyGameDataProvider : SharedMemoryGameDataProviderBase<ACRallySharedMemoryReader, ACRallyTelemetry>
{
    protected override ACRallySharedMemoryReader CreateReader() => new();

    protected override bool HasDataChanged(ACRallyTelemetry current, ACRallyTelemetry last)
    {
        // The physics page bumps its packet id on every frame, which also catches
        // changes the dashboard cares about while the engine sits at a steady rpm.
        return current.PacketId != last.PacketId;
    }
}
