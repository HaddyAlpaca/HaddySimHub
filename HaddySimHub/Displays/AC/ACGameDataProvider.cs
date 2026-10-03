using HaddySimHub.Capture;

namespace HaddySimHub.Displays.AC;

/// <summary>
/// Provides Assetto Corsa telemetry from shared memory.
/// </summary>
public class ACGameDataProvider : SharedMemoryGameDataProviderBase<ACSharedMemoryReader, ACTelemetry>
{
    private readonly TelemetryCapture? _capture;

    public ACGameDataProvider(TelemetryCapture? capture = null)
    {
        _capture = capture;
    }

    protected override ACSharedMemoryReader CreateReader() => new(_capture);

    protected override bool HasDataChanged(ACTelemetry current, ACTelemetry last)
    {
        // The physics page bumps its packet id on every frame, which also catches
        // changes the dashboard cares about while the engine sits at a steady rpm.
        return current.PacketId != last.PacketId;
    }
}
