namespace HaddySimHub.Models;

public record TrackPosition
{
    public string DriverName { get; init; } = string.Empty;
    public float LapDistPct { get; init; }
    public TrackPositionStatus Status { get; init; }
}