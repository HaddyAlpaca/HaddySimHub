using System.Runtime.InteropServices;

namespace HaddySimHub.Raceroom.Data;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct PushToPass
{
    public int Available;
    public int Engaged;
    public int AmountLeft;
    public float EngagedTimeLeft;
    public float WaitTimeLeft;
}
