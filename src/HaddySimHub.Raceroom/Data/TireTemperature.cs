using System.Runtime.InteropServices;

namespace HaddySimHub.Raceroom.Data;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct TireTemperature<T>
{
    public T Left;
    public T Center;
    public T Right;
}
