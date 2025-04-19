namespace HaddySimHub.Models;

public record TrackPosition
{
    public string Name { get; init; }
    public string CarNumber { get; init; }
    public string License { get; init; }
    public string LicenseColor { get; init; }
    public float LapDistPct { get; init; }
    public TrackPositionStatus Status { get; init; }
}
