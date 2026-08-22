using System.Runtime.InteropServices;

namespace HaddySimHub.Displays.ACRally;

/// <summary>
/// A 3D vector as laid out in the Assetto Corsa shared memory pages.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct ACRallyVector3
{
    public float X;
    public float Y;
    public float Z;
}

/// <summary>
/// A per-wheel value block (front left, front right, rear left, rear right).
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct ACRallyWheelData
{
    public float FrontLeft;
    public float FrontRight;
    public float RearLeft;
    public float RearRight;
}

/// <summary>
/// Mirrors <c>SPageFilePhysics</c> as published by Assetto Corsa Rally on
/// <c>Local\acpmf_physics</c> (~333 Hz).
/// </summary>
/// <remarks>
/// Every field must stay in place even when unused: the struct is read as a raw
/// memory image, so a missing or reordered field corrupts everything after it.
/// </remarks>
[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct ACRallyPhysics
{
    public int PacketId;
    public float Gas;
    public float Brake;
    public float Fuel;

    /// <summary>Gear index: 0 = reverse, 1 = neutral, 2 = first gear.</summary>
    public int Gear;

    public int Rpms;
    public float SteerAngle;
    public float SpeedKmh;
    public ACRallyVector3 Velocity;
    public ACRallyVector3 AccG;
    public ACRallyWheelData WheelSlip;
    public ACRallyWheelData WheelLoad;
    public ACRallyWheelData WheelsPressure;
    public ACRallyWheelData WheelAngularSpeed;
    public ACRallyWheelData TyreWear;
    public ACRallyWheelData TyreDirtyLevel;
    public ACRallyWheelData TyreCoreTemperature;
    public ACRallyWheelData CamberRad;
    public ACRallyWheelData SuspensionTravel;
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
    public ACRallyVector3 LocalAngularVelocity;
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
    public ACRallyWheelData BrakeTemp;
    public float Clutch;
    public ACRallyWheelData TyreTempI;
    public ACRallyWheelData TyreTempM;
    public ACRallyWheelData TyreTempO;
    public int IsAIControlled;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public ACRallyVector3[] TyreContactPoint;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public ACRallyVector3[] TyreContactNormal;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public ACRallyVector3[] TyreContactHeading;

    public float BrakeBias;
    public ACRallyVector3 LocalVelocity;
    public int P2PActivation;
    public int P2PStatus;
    public float CurrentMaxRpm;
    public ACRallyWheelData Mz;
    public ACRallyWheelData Fx;
    public ACRallyWheelData Fy;
    public ACRallyWheelData SlipRatio;
    public ACRallyWheelData SlipAngle;
    public int TcInAction;
    public int AbsInAction;
    public ACRallyWheelData SuspensionDamage;
    public ACRallyWheelData TyreTemp;
    public float WaterTemperature;
    public ACRallyWheelData BrakePressure;
    public int FrontBrakeCompound;
    public int RearBrakeCompound;
    public ACRallyWheelData PadLife;
    public ACRallyWheelData DiscLife;
    public int IgnitionOn;
    public int StarterEngineOn;
    public int IsEngineRunning;
    public float KerbVibration;
    public float SlipVibrations;
    public float GVibrations;
    public float AbsVibrations;
}
