namespace HaddySimHub.GameData;

public record TrackPosition
{
    public float LapDistPct { get; init; }
    public TrackPositionStatus Status { get; init; }
}