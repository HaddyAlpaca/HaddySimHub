using System.Runtime.InteropServices;

namespace HaddySimHub.Raceroom.Data;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct AidSettings
{
    // ABS; -1 = N/A, 0 = off, 1 = on, 5 = currently active
    public int Abs;
    // TC; -1 = N/A, 0 = off, 1 = on, 5 = currently active
    public int Tc;
    // ESP; -1 = N/A, 0 = off, 1 = on low, 2 = on medium, 3 = on high, 5 = currently active
    public int Esp;
    // Countersteer; -1 = N/A, 0 = off, 1 = on, 5 = currently active
    public int Countersteer;
    // Cornering; -1 = N/A, 0 = off, 1 = on, 5 = currently active
    public int Cornering;
}
