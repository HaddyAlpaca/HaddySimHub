namespace HaddySimHub.IRacing;

/// <summary>
/// Completed sector information.
/// </summary>
internal class CompletedSector
{
    /// <summary>
    /// Gets sector number.
    /// </summary>
    public int SectorNum { get; init; }

    /// <summary>
    /// Gets sector time.
    /// </summary>
    public TimeSpan SectorTime { get; init; }
}