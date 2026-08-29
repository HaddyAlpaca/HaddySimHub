namespace HaddySimHub.Models;

/// <summary>
/// What is driving the course deviation indicator.
/// </summary>
/// <remarks>
/// The navigation radio wins over the flight plan when it is receiving a station,
/// which is how a pilot flying an approach expects the needle to behave.
/// </remarks>
public enum CourseDeviationSource
{
    /// <summary>No guidance at all: no station received and no flight plan.</summary>
    None,

    /// <summary>Deviation from the active flight plan leg.</summary>
    Gps,

    /// <summary>Deviation from the selected radial of a VOR.</summary>
    Vor,

    /// <summary>Deviation from a localizer, which may also carry a glideslope.</summary>
    Localizer,
}
