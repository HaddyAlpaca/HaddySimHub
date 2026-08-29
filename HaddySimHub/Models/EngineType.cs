namespace HaddySimHub.Models;

/// <summary>
/// Mirrors the MSFS <c>ENGINE TYPE</c> simvar, values included. The dashboard uses it
/// to label the primary engine readout: turbines report N1, pistons a percentage of
/// maximum RPM.
/// </summary>
public enum EngineType
{
    Piston,
    Jet,
    None,
    HeloBellTurbine,
    Unsupported,
    Turboprop,
}
