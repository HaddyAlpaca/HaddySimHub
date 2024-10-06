namespace HaddySimHub.iRacing;

/// <summary>
/// Sector timing information.
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
    public double SectorStartTime { get; init; }

    /// <summary>
    /// Gets lap time when exiting the sector.
    /// </summary>
    public double SectorEndTime { get; init; }

    /// <summary>
    /// Gets sector time in seconds.
    /// </summary>
    public double SectorTime
    {
        get
        {
            if (this.SectorStartTime == 0 || this.SectorEndTime == 0)
            {
                return 0;
            }

            return this.SectorEndTime - this.SectorStartTime;
        }
    }

    /// <summary>
    /// Gets the sessions best sector time.
    /// </summary>
    public double SessionBestTime { get; init; }
}