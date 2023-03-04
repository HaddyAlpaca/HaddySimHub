using System.Runtime.InteropServices;

namespace HaddySimHub.Raceroom.Data;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct BrakeTemp
{
    public float CurrentTemp;
    public float OptimalTemp;
    public float ColdTemp;
    public float HotTemp;
}
