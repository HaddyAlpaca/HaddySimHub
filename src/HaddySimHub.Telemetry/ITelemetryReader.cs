namespace HaddySimHub.Telemetry;

public interface ITelemetryReader
{
    string ProcessName { get; }
    object ReadTelemetry();
}