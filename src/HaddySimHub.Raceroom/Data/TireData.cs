using System.Runtime.InteropServices;

namespace HaddySimHub.Raceroom.Data;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct TireData<T>
{
    public T FrontLeft;
    public T FrontRight;
    public T RearLeft;
    public T RearRight;
}
