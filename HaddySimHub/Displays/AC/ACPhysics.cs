using System.Runtime.InteropServices;

namespace HaddySimHub.Displays.AC;

/// <summary>
/// A 3D vector as laid out in the Assetto Corsa shared memory pages.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct ACVector3
{
    public float X;
    public float Y;
    public float Z;
}

/// <summary>
/// Mirrors <c>SPageFilePhysics</c> as published by Assetto Corsa on
/// <c>Local\acpmf_physics</c>.
/// </summary>
/// <remarks>
/// Every field must stay in place even when unused: the page is read as a raw
/// memory image, so a missing or reordered field corrupts everything after it.
/// The struct stops at the last field Assetto Corsa itself publishes; Competizione
/// and Rally continue past this point with fields of their own.
/// </remarks>
[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct ACPhysics
{
    public int PacketId;
    public float Gas;
    public float Brake;

    /// <summary>Fuel left in the tank, in litres.</summary>
    public float Fuel;

    /// <summary>Gear index: 0 = reverse, 1 = neutral, 2 = first gear.</summary>
    public int Gear;

    public int Rpms;
    public float SteerAngle;
    public float SpeedKmh;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
    public float[] Velocity;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
    public float[] AccG;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public float[] WheelSlip;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public float[] WheelLoad;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public float[] WheelsPressure;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public float[] WheelAngularSpeed;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public float[] TyreWear;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public float[] TyreDirtyLevel;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public float[] TyreCoreTemperature;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public float[] CamberRad;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public float[] SuspensionTravel;

    public float Drs;
    public float TC;
    public float Heading;
    public float Pitch;
    public float Roll;
    public float CgHeight;

    /// <summary>Damage per zone: front, rear, left, right, centre.</summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
    public float[] CarDamage;

    public int NumberOfTyresOut;
    public int PitLimiterOn;
    public float Abs;
    public float KersCharge;
    public float KersInput;
    public int AutoShifterOn;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
    public float[] RideHeight;

    public float TurboBoost;
    public float Ballast;
    public float AirDensity;
    public float AirTemp;
    public float RoadTemp;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
    public float[] LocalAngularVelocity;

    public float FinalFF;
    public float PerformanceMeter;
    public int EngineBrake;
    public int ErsRecoveryLevel;
    public int ErsPowerLevel;
    public int ErsHeatCharging;
    public int ErsIsCharging;
    public float KersCurrentKJ;
    public int DrsAvailable;
    public int DrsEnabled;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public float[] BrakeTemp;

    public float Clutch;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public float[] TyreTempI;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public float[] TyreTempM;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public float[] TyreTempO;

    public int IsAIControlled;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public ACVector3[] TyreContactPoint;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public ACVector3[] TyreContactNormal;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public ACVector3[] TyreContactHeading;

    public float BrakeBias;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
    public float[] LocalVelocity;
}
