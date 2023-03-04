using System.Runtime.InteropServices;

namespace HaddySimHub.Raceroom.Data;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct RaceDuration<T>
{
    public T Race1;
    public T Race2;
    public T Race3;
}
