using System.Runtime.InteropServices;

namespace HaddySimHub.Raceroom.Data;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct CarDamage
{
    // Range: 0.0 - 1.0
    // Note: -1.0 = N/A
    public float Engine;

    // Range: 0.0 - 1.0
    // Note: -1.0 = N/A
    public float Transmission;

    // Range: 0.0 - 1.0
    // Note: A bit arbitrary at the moment. 0.0 doesn't necessarily mean completely destroyed.
    // Note: -1.0 = N/A
    public float Aerodynamics;

    // Range: 0.0 - 1.0
    // Note: -1.0 = N/A
    public float Suspension;

    // Reserved data
    public float Unused1;
    public float Unused2;
}
