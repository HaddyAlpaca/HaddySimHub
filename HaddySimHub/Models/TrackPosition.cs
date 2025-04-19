namespace HaddySimHub.Models;

public record TimingEntry
{
    public string Name { get; init; } = string.Empty;
    public string CarNumber { get; init; } = string.Empty;
    public string License { get; init; } = string.Empty;
    public string LicenseColor { get; init; } = string.Empty;
    public long IRating { get; init; }
    public int Lap { get; init; }
    public float DistancePct { get; init; }
}
