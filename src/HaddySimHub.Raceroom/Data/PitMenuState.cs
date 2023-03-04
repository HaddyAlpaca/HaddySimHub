using System.Runtime.InteropServices;

namespace HaddySimHub.Raceroom.Data;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct PitMenuState
{
    // Pit menu preset
    public int Preset;

    // Pit menu actions
    public int Penalty;
    public int Driverchange;
    public int Fuel;
    public int FrontTires;
    public int RearTires;
    public int FrontWing;
    public int RearWing;
    public int Suspension;

    // Pit menu buttons
    public int ButtonTop;
    public int ButtonBottom;
}
