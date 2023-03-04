using System.Runtime.InteropServices;

namespace HaddySimHub.Raceroom.Data;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct CutTrackPenalties
{
    public int DriveThrough;
    public int StopAndGo;
    public int PitStop;
    public int TimeDeduction;
    public int SlowDown;
}
