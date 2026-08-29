namespace HaddySimHub.Models;

/// <summary>
/// Mirrors the MSFS <c>NAV TOFROM</c> simvar, values included: whether the selected
/// radial leads to the station or away from it. Only meaningful for a VOR.
/// </summary>
public enum NavToFrom
{
    Off,
    To,
    From,
}
