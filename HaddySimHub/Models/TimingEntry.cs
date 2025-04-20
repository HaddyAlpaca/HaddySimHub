namespace HaddySimHub.Models;

public record TimingEntry
{
    public int Position { get; init; }
    public string DriverName { get; init; } = string.Empty;
    public string CarNumber { get; init; } = string.Empty;
    public string License { get; init; } = string.Empty;
    public string LicenseColor { get; init; } = string.Empty;
    public long IRating { get; init; }
    public int LapsCompleted { get; init; }
    public float DistancePct { get; init; }
    public bool IsPlayer { get; init; }
    public bool IsSafetyCar { get; init; }
    public bool IsInPits { get; init; }
}
