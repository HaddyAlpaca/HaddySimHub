using System.Runtime.InteropServices;

namespace HaddySimHub.Displays.ACC;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct ACCPhysics
{
    public int PacketId;
    public float Gas;
    public float Brake;
    public float Fuel;
    public int Gear;
    public int Rpms;
    public float SteerAngle;
    public float SpeedKmh;
    public ACCVector3 Velocity;
    public ACCVector3 AccG;
    public ACCWheelData WheelSlip;
    public ACCWheelData WheelLoad;
    public ACCWheelData WheelsPressure;
    public ACCWheelData WheelAngularSpeed;
    public ACCWheelData TyreWear;
    public ACCWheelData TyreDirtyLevel;
    public ACCWheelData TyreCoreTemperature;
    public ACCWheelData CamberRad;
    public ACCWheelData SuspensionTravel;
    public float Drs;
    public float TC;
    public float Heading;
    public float Pitch;
    public float Roll;
    public float CgHeight;
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
    public ACCVector3 LocalAngularVelocity;
    public float FinalFF;
    public float PerformanceMeter;
    public int EngineBrake;
    public int ErsRecoveryLevel;
    public int ErsPowerLevel;
    public int ErsHeatCharging;
    public int ErsisCharging;
    public float KersCurrentKJ;
    public int DrsAvailable;
    public int DrsEnabled;
    public ACCWheelData BrakeTemp;
    public float Clutch;
    public ACCWheelData TyreTempI;
    public ACCWheelData TyreTempM;
    public ACCWheelData TyreTempO;
    public int IsAIControlled;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public ACCVector3[] TyreContactPoint;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public ACCVector3[] TyreContactNormal;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public ACCVector3[] TyreContactHeading;
    public float BrakeBias;
    public ACCVector3 LocalVelocity;
    public int P2PActivation;
    public int P2PStatus;
    public float CurrentMaxRpm;
    public ACCWheelData Mz;
    public ACCWheelData Fx;
    public ACCWheelData Fy;
    public ACCWheelData SlipRatio;
    public ACCWheelData SlipAngle;
    public int TcinAction;
    public int AbsInAction;
    public ACCWheelData SuspensionDamage;
    public ACCWheelData TyreTemp;
    public float WaterTemp;
    public ACCWheelData BrakePressure;
    public int FrontBrakeCompound;
    public int RearBrakeCompound;
    public ACCWheelData PadLife;
    public ACCWheelData DiscLife;
    public int IgnitionOn;
    public int StarterEngineOn;
    public int IsEngineRunning;
    public float KerbVibration;
    public float SlipVibrations;
    public float GVibrations;
    public float AbsVibrations;
}

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct ACCVector3
{
    public float X;
    public float Y;
    public float Z;
}

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct ACCWheelData
{
    public float FrontLeft;
    public float FrontRight;
    public float RearLeft;
    public float RearRight;
}
