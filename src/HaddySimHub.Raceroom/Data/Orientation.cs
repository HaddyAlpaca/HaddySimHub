using System.Runtime.InteropServices;

namespace HaddySimHub.Raceroom.Data;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct Orientation<T>
{
    public T Pitch;
    public T Yaw;
    public T Roll;
}
