namespace HaddySimHub.Displays.ACC;

/// <summary>
/// Provides Assetto Corsa Competizione telemetry from shared memory.
/// </summary>
public class ACCGameDataProvider : SharedMemoryGameDataProviderBase<ACCSharedMemoryReader, ACCTelemetry>
{
    private DateTime _lastDataLog = DateTime.MinValue;

    protected override string ProviderName => "ACC";

    protected override ACCSharedMemoryReader CreateReader() => new();

    protected override bool HasDataChanged(ACCTelemetry current, ACCTelemetry last)
    {
        // The physics page bumps its packet id on every frame, which also catches
        // changes the dashboard cares about while the engine sits at a steady rpm.
        return current.PacketId != last.PacketId;
    }

    protected override void OnDataChanged(ACCTelemetry telemetry)
    {
        if ((DateTime.Now - _lastDataLog).TotalSeconds > 5)
        {
            Logger.Debug($"[ACC] Telemetry: RPM={telemetry.Rpms:F0} Speed={telemetry.SpeedKmh:F1} Gear={telemetry.Gear}");
            _lastDataLog = DateTime.Now;
        }
    }
}
