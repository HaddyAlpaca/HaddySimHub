using System.Runtime.InteropServices;

namespace HaddySimHub.Raceroom.Data;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct Vector3<T>
{
    public T X;
    public T Y;
    public T Z;
}
