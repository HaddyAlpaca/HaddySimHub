namespace HaddySimHub.IRacing;

/// <summary>
/// Completed sector information.
/// </summary>
internal class Sector
{
    /// <summary>
    /// Gets lap number.
    /// </summary>
    public int LapNum { get; init; }

    /// <summary>
    /// Gets sector number.
    /// </summary>
    public int SectorNum { get; init; }

    /// <summary>
    /// Gets lap time when entering the sector.
    /// </summary>
    public float SectorStartTime { get; init; }

    /// <summary>
    /// Gets lap time when exiting the sector.
    /// </summary>
    public float SectorEndTime { get; init; }

    /// <summary>
    /// Gets sector time in seconds.
    /// </summary>
    public float SectorTime => this.SectorEndTime - this.SectorStartTime;
}