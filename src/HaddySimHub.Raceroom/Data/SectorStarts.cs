using System.Runtime.InteropServices;

namespace HaddySimHub.Raceroom.Data;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct SectorStarts<T>
{
    public T Sector1;
    public T Sector2;
    public T Sector3;
}
