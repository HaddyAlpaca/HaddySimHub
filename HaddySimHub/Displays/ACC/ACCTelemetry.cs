using System.Runtime.InteropServices;

namespace HaddySimHub.Displays.ACC;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct ACCTelemetry
{
    public int PacketId;
    
    public float Gas;
    public float Brake;
    public float Fuel;
    public int Gear;
    public int Rpms;
    public float SteerAngle;
    public float SpeedKmh;
    
    public float VelocityX;
    public float VelocityY;
    public float VelocityZ;
    public float AccGX;
    public float AccGY;
    public float AccGZ;
    
    public float WheelSlipFl;
    public float WheelSlipFr;
    public float WheelSlipRl;
    public float WheelSlipRr;
    
    public float WheelLoadFl;
    public float WheelLoadFr;
    public float WheelLoadRl;
    public float WheelLoadRr;
    
    public float WheelPressureFl;
    public float WheelPressureFr;
    public float WheelPressureRl;
    public float WheelPressureRr;
    
    public float WheelAngularSpeedFl;
    public float WheelAngularSpeedFr;
    public float WheelAngularSpeedRl;
    public float WheelAngularSpeedRr;
    
    public float TyreWearFl;
    public float TyreWearFr;
    public float TyreWearRl;
    public float TyreWearRr;
    
    public float TyreCoreTempFl;
    public float TyreCoreTempFr;
    public float TyreCoreTempRl;
    public float TyreCoreTempRr;
    
    public float BrakeTempFl;
    public float BrakeTempFr;
    public float BrakeTempRl;
    public float BrakeTempRr;
    
    public float CamberRadFl;
    public float CamberRadFr;
    public float CamberRadRl;
    public float CamberRadRr;
    
    public float SuspensionTravelFl;
    public float SuspensionTravelFr;
    public float SuspensionTravelRl;
    public float SuspensionTravelRr;
    
    public float Drs;
    public float TC;
    public float Heading;
    public float Pitch;
    public float Roll;
    public float CgHeight;
    
    public float FrontLeftDamage;
    public float FrontRightDamage;
    public float RearLeftDamage;
    public float RearRightDamage;
    public float CenterDamage;
    
    public int NumberOfTyresOut;
    public int PitLimiterOn;
    public float Abs;
    public float KersCharge;
    public float KersInput;
    public int AutoShifterOn;
    
    public float RideHeightFront;
    public float RideHeightRear;
    public float TurboBoost;
    public float Ballast;
    public float AirDensity;
    public float AirTemp;
    public float RoadTemp;
    
    public float LocalAngularVelocityX;
    public float LocalAngularVelocityY;
    public float LocalAngularVelocityZ;
    
    public float FinalFF;
    public float PerformanceMeter;
    public int EngineBrake;
    public int DrsAvailable;
    public int DrsEnabled;
    public float Clutch;
    
    public float TyreTempIFl;
    public float TyreTempIFr;
    public float TyreTempIRl;
    public float TyreTempIRr;
    
    public float TyreTempMFl;
    public float TyreTempMFr;
    public float TyreTempMRl;
    public float TyreTempMRr;
    
    public float TyreTempOFl;
    public float TyreTempOFr;
    public float TyreTempORl;
    public float TyreTempORr;
    
    public int IsAIControlled;
    public float BrakeBias;
    public float LocalVelocityX;
    public float LocalVelocityY;
    public float LocalVelocityZ;
    public int P2PActivation;
    public int P2PStatus;
    public float MaxRpm;
    public int TcinAction;
    public int AbsInAction;
    public float WaterTemp;
    public int FrontBrakeCompound;
    public int RearBrakeCompound;
    
    public float PadLifeFl;
    public float PadLifeFr;
    public float PadLifeRl;
    public float PadLifeRr;
    
    public float DiscLifeFl;
    public float DiscLifeFr;
    public float DiscLifeRl;
    public float DiscLifeRr;
    
    public int IgnitionOn;
    public int StarterEngineOn;
    public int IsEngineRunning;
    public float KerbVibration;
    public float SlipVibrations;
    public float GVibrations;
    public float AbsVibrations;
    
    public int Status;
    public int SessionType;
    public int CurrentTimeMs;
    public int LastTimeMs;
    public int BestTimeMs;
    public int CurrentLap;
    public int Position;
    public int SessionTimeLeftMs;
    public float DistanceTraveled;
    public int IsInPit;
    public int CurrentSectorIndex;
    public int LastSectorTimeMs;
    public int NumberOfLaps;
    public string TyreCompound;
    public float NormalizedCarPosition;
    public float PenaltyTime;
    public int Flag;
    public int Penalty;
    public int IdealLineOn;
    public int IsInPitLane;
    public int MandatoryPitDone;
    public float WindSpeed;
    public float WindDirection;
    public int TcLevel;
    public int TcCutLevel;
    public int EngineMap;
    public int AbsLevel;
    public float FuelPerLap;
    public int RainLight;
    public int FlashingLight;
    public float ExhaustTemp;
    public float UsedFuel;
    public int DeltaMs;
    public int IsDeltaPositive;
    public int IsValidLap;
    public float FuelEstimatedLaps;
    public int MissingMandatoryPits;
    public int CurrentTyreSet;
    public int StrategyTyreSet;
    public int GapAheadMs;
    public int GapBehindMs;
    public int TrackGripStatus;
    public int RainIntensity;
}
