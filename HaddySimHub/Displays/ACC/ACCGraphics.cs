using System.Runtime.InteropServices;

namespace HaddySimHub.Displays.ACC;

public enum ACCStatus : int
{
    Off = 0,
    Replay = 1,
    Live = 2,
    Pause = 3
}

public enum ACCSessionType : int
{
    Unknown = -1,
    Practice = 0,
    Qualifying = 1,
    Race = 2,
    Hotlap = 3,
    TimeAttack = 4,
    Drift = 5,
    Drag = 6,
    Hotstint = 7,
    HotlapSuperpole = 8
}

public enum ACCFlagType : int
{
    NoFlag = 0,
    BlueFlag = 1,
    YellowFlag = 2,
    BlackFlag = 3,
    WhiteFlag = 4,
    CheckeredFlag = 5,
    PenaltyFlag = 6,
    GreenFlag = 7,
    OrangeFlag = 8
}

public enum ACCPenaltyType : int
{
    NoPenalty = 0,
    DriveThrough_Cutting = 1,
    StopAndGo_10_Cutting = 2,
    StopAndGo_20_Cutting = 3,
    StopAndGo_30_Cutting = 4,
    Disqualified_Cutting = 5,
    RemoveBestLaptime_Cutting = 6,
    DriveThrough_PitSpeeding = 7,
    StopAndGo_10_PitSpeeding = 8,
    StopAndGo_20_PitSpeeding = 9,
    StopAndGo_30_PitSpeeding = 10,
    Disqualified_PitSpeeding = 11,
    RemoveBestLaptime_PitSpeeding = 12,
    Disqualified_IgnoredMandatoryPit = 13,
    PostRaceTime = 14,
    Disqualified_Trolling = 15,
    Disqualified_PitEntry = 16,
    Disqualified_PitExit = 17,
    Disqualified_WrongWay = 22
}

[StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Unicode)]
public struct ACCGraphics
{
    public int PacketId;
    public ACCStatus Status;
    public ACCSessionType SessionType;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
    public string CurrentTimeStr;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
    public string LastTimeStr;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
    public string BestTimeStr;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
    public string LastSectorTimeStr;
    public int CompletedLap;
    public int Position;
    public int CurrentTime;
    public int LastTime;
    public int BestTime;
    public float SessionTimeLeft;
    public float DistanceTraveled;
    public int IsInPit;
    public int CurrentSectorIndex;
    public int LastSectorTime;
    public int NumberOfLaps;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
    public string TyreCompound;
    public float NormalizedCarPosition;
    public int ActiveCars;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 60)]
    public ACCVector3[] CarCoordinates;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 60)]
    public int[] CarIds;
    public int PlayerCarId;
    public float PenaltyTime;
    public ACCFlagType Flag;
    public ACCPenaltyType Penalty;
    public int IdealLineOn;
    public int IsInPitLane;
    public int MandatoryPitDone;
    public float WindSpeed;
    public float WindDirection;
    public int IsSetupMenuVisible;
    public int MainDisplayIndex;
    public int SecondaryDisplayIndex;
    public int TcLevel;
    public int TcCutLevel;
    public int EngineMap;
    public int AbsLevel;
    public float FuelPerLap;
    public int RainLight;
    public int FlashingLight;
    public int LightStage;
    public float ExhaustTemp;
    public int WiperStage;
    public int DriverStintTotalTimeLeft;
    public int DriverStintTimeLeft;
    public int RainTyres;
    public int SessionIndex;
    public float UsedFuel;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
    public string DeltaLapTimeStr;
    public int DeltaLapTime;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
    public string EstimatedLapTimeStr;
    public int EstimatedLapTime;
    public int IsDeltaPositive;
    public int LastSectorTime2;
    public int IsValidLap;
    public float FuelEstimatedLaps;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
    public string TrackStatus;
    public int MissingMandatoryPits;
    public int Clock;
    public int DirectionLightLeft;
    public int DirectionLightRight;
    public int GlobalYellow;
    public int GlobalYellowS1;
    public int GlobalYellowS2;
    public int GlobalYellowS3;
    public int GlobalWhite;
    public int GlobalGreen;
    public int GlobalChequered;
    public int GlobalRed;
    public int MfdTyreSet;
    public float MfdFuelToAdd;
    public ACCWheelData MfdTyrePressure;
    public int TrackGripStatus;
    public int RainIntensity;
    public int RainIntensityIn10Min;
    public int RainIntensityIn30Min;
    public int CurrentTyreSet;
    public int StrategyTyreSet;
    public int GapAhead;
    public int GapBehind;
}
