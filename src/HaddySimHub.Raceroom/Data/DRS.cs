using System;
using System.Runtime.InteropServices;

namespace HaddySimHub.Raceroom.Data;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct DRS
{
    // If DRS is equipped and allowed
    // 0 = No, 1 = Yes, -1 = N/A
    public int Equipped;
    // Got DRS activation left
    // 0 = No, 1 = Yes, -1 = N/A
    public int Available;
    // Number of DRS activations left this lap
    // Note: In sessions with 'endless' amount of drs activations per lap this value starts at int32::max
    // -1 = N/A
    public int NumActivationsLeft;
    // DRS engaged
    // 0 = No, 1 = Yes, -1 = N/A
    public int Engaged;
}
