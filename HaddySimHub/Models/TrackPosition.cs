namespace HaddySimHub.Models;

public record TrackPosition
{
    public string Name { get; init; } = string.Empty;
    public string CarNumber { get; init; } = string.Empty;
    public string License { get; init; } = string.Empty;
    public string LicenseColor { get; init; } = string.Empty;
    public long IRating { get; init; }
    public float LapDistPct { get; init; }
    public TrackPositionStatus Status { get; init; }
}
