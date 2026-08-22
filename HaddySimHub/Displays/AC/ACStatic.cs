using System.Runtime.InteropServices;

namespace HaddySimHub.Displays.AC;

/// <summary>
/// Mirrors the leading part of <c>SPageFileStatic</c> as published by Assetto
/// Corsa on <c>Local\acpmf_static</c>. Written once when a session is loaded.
/// </summary>
/// <remarks>
/// The struct stops after <see cref="MaxFuel"/>, which is the last field the
/// display needs. Fields beyond it are shared with Competizione in some releases
/// and not in others, so leaving them unmapped avoids depending on that.
/// </remarks>
[StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Unicode)]
public struct ACStatic
{
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
    public string SmVersion;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
    public string AcVersion;

    public int NumberOfSessions;
    public int NumCars;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
    public string CarModel;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
    public string Track;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
    public string PlayerName;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
    public string PlayerSurname;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 34)]  // 33 chars + 1 char of 2-byte padding
    public string PlayerNick;

    public int SectorCount;
    public float MaxTorque;
    public float MaxPower;

    /// <summary>Engine rev limit for the current car.</summary>
    public int MaxRpm;

    /// <summary>Fuel tank capacity in litres.</summary>
    public float MaxFuel;
}
