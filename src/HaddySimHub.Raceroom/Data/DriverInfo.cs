using HaddySimHub.Raceroom.Enums;
using System.Runtime.InteropServices;

namespace HaddySimHub.Raceroom.Data;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct DriverInfo
{
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
    public byte[] Name; // UTF-8
    public int CarNumber;
    public int ClassId;
    public int ModelId;
    public int TeamId;
    public int LiveryId;
    public int ManufacturerId;
    public int UserId;
    public int SlotId;
    public int ClassPerformanceIndex;
    public EngineType EngineType;

    public int Unused1;
    public int Unused2;
}
