using System.Runtime.InteropServices;

namespace HaddySimHub.Displays.ACRally;

public enum ACRallyStatus : int
{
    Off = 0,
    Replay = 1,
    Live = 2,
    Pause = 3,
}

public enum ACRallySessionType : int
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
    HotlapSuperpole = 8,
}

/// <summary>
/// Mirrors <c>SPageFileGraphic</c> as published by Assetto Corsa Rally on
/// <c>Local\acpmf_graphics</c> (~60 Hz).
/// </summary>
/// <remarks>
/// Field order and the string paddings below are part of the binary layout;
/// changing them silently shifts every field that follows.
/// </remarks>
[StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Unicode)]
public struct ACRallyGraphics
{
    public int PacketId;
    public ACRallyStatus Status;
    public ACRallySessionType SessionType;

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

    /// <summary>Elapsed time on the current stage in milliseconds.</summary>
    public int CurrentTime;

    public int LastTime;
    public int BestTime;
    public float SessionTimeLeft;

    /// <summary>Distance covered on the current stage in metres.</summary>
    public float DistanceTraveled;

    public int IsInPit;
    public int CurrentSectorIndex;

    /// <summary>Duration of the most recently completed sector in milliseconds.</summary>
    public int LastSectorTime;

    public int NumberOfLaps;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 34)]  // 33 chars + 1 char of 2-byte padding
    public string TyreCompound;

    public float ReplayTimeMultiplier;

    /// <summary>Position along the stage spline, 0.0 at the start and 1.0 at the finish.</summary>
    public float NormalizedCarPosition;

    public int ActiveCars;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 60)]
    public ACRallyVector3[] CarCoordinates;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 60)]
    public int[] CarIds;

    public int PlayerCarId;
    public float PenaltyTime;
    public int Flag;
    public int Penalty;
    public int IdealLineOn;
    public int IsInPitLane;
    public float SurfaceGrip;
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

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]  // 15 chars + 1 char of 2-byte padding
    public string DeltaLapTimeStr;

    public int DeltaLapTime;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]  // 15 chars + 1 char of 2-byte padding
    public string EstimatedLapTimeStr;

    public int EstimatedLapTime;
    public int IsDeltaPositive;
    public int ISplit;
    public int IsValidLap;
    public float FuelEstimatedLaps;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 34)]  // 33 chars + 1 char of 2-byte padding
    public string TrackStatus;

    public int MissingMandatoryPits;
    public float Clock;
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
    public ACRallyWheelData MfdTyrePressure;
    public int TrackGripStatus;
    public int RainIntensity;
    public int RainIntensityIn10Min;
    public int RainIntensityIn30Min;
    public int CurrentTyreSet;
    public int StrategyTyreSet;
    public int GapAhead;
    public int GapBehind;
}
