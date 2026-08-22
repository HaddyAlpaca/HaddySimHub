using System.Runtime.InteropServices;

namespace HaddySimHub.Displays.ACRally;

/// <summary>
/// Mirrors <c>SPageFileStatic</c> as published by Assetto Corsa Rally on
/// <c>Local\acpmf_static</c>. Written once when a session is loaded.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Unicode)]
public struct ACRallyStatic
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

    public float MaxFuel;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public float[] SuspensionMaxTravel;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public float[] TyreRadius;

    public float MaxTurboBoost;
    public float Deprecated1;
    public float Deprecated2;
    public int PenaltiesEnabled;
    public float AidFuelRate;
    public float AidTireRate;
    public float AidMechanicalDamage;
    public int AidAllowTyreBlankets;
    public float AidStability;
    public int AidAutoClutch;
    public int AidAutoBlip;
    public int HasDRS;
    public int HasERS;
    public int HasKERS;
    public float KersMaxJ;
    public int EngineBrakeSettingsCount;
    public int ErsPowerControllerCount;

    /// <summary>Length of the stage spline in metres.</summary>
    public float TrackSplineLength;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 34)]  // 33 chars + 1 char of 2-byte padding
    public string TrackConfiguration;

    public float ErsMaxJ;
    public int IsTimedRace;
    public int HasExtraLap;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 34)]  // 33 chars + 1 char of 2-byte padding
    public string CarSkin;

    public int ReversedGridPositions;
    public int PitWindowStart;
    public int PitWindowEnd;
    public int IsOnline;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
    public string DryTyresName;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
    public string WetTyresName;
}
