using HaddySimHub.Raceroom.Enums;
using System.Runtime.InteropServices;

namespace HaddySimHub.Raceroom.Data;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct DriverData
{
    public DriverInfo DriverInfo;
    public FinishStatus FinishStatus;
    public int Place;
    // Based on performance index
    public int PlaceClass;
    public float LapDistance;
    public Vector3<float> Position;
    public int TrackSector;
    public int CompletedLaps;
    public int CurrentLapValid;
    public float LapTimeCurrentSelf;
    public Sectors<float> SectorTimeCurrentSelf;
    public Sectors<float> SectorTimePreviousSelf;
    public Sectors<float> SectorTimeBestSelf;
    public float TimeDeltaFront;
    public float TimeDeltaBehind;
    public PitStopStatus PitStopStatus;
    public int InPitlane;

    public int NumPitstops;

    public CutTrackPenalties Penalties;

    public float CarSpeed;
    public TireType TireTypeFront;
    public TireType TireTypeRear;
    public TireSubtype TireSubtypeFront;
    public TireSubtype TireSubtypeRear;

    public float BasePenaltyWeight;
    public float AidPenaltyWeight;

    public DrsState DrsState;
    public PtpState PtpState;

    public PenaltyType PenaltyType;

    // Based on the PenaltyType you can assume the reason is:

    // DriveThroughPenaltyInvalid = 0,
    // DriveThroughPenaltyCutTrack = 1,
    // DriveThroughPenaltyPitSpeeding = 2,
    // DriveThroughPenaltyFalseStart = 3,
    // DriveThroughPenaltyIgnoredBlue = 4,
    // DriveThroughPenaltyDrivingTooSlow = 5,
    // DriveThroughPenaltyIllegallyPassedBeforeGreen = 6,
    // DriveThroughPenaltyIllegallyPassedBeforeFinish = 7,
    // DriveThroughPenaltyIllegallyPassedBeforePitEntrance = 8,
    // DriveThroughPenaltyIgnoredSlowDown = 9,
    // DriveThroughPenaltyMax = 10

    // StopAndGoPenaltyInvalid = 0,
    // StopAndGoPenaltyCutTrack1st = 1,
    // StopAndGoPenaltyCutTrackMult = 2,
    // StopAndGoPenaltyYellowFlagOvertake = 3,
    // StopAndGoPenaltyMax = 4

    // PitstopPenaltyInvalid = 0,
    // PitstopPenaltyIgnoredPitstopWindow = 1,
    // PitstopPenaltyMax = 2

    // ServableTimePenaltyInvalid = 0,
    // ServableTimePenaltyServedMandatoryPitstopLate = 1,
    // ServableTimePenaltyIgnoredMinimumPitstopDuration = 2,
    // ServableTimePenaltyMax = 3

    // SlowDownPenaltyInvalid = 0,
    // SlowDownPenaltyCutTrack1st = 1,
    // SlowDownPenaltyCutTrackMult = 2,
    // SlowDownPenaltyMax = 3

    // DisqualifyPenaltyInvalid = -1,
    // DisqualifyPenaltyFalseStart = 0,
    // DisqualifyPenaltyPitlaneSpeeding = 1,
    // DisqualifyPenaltyWrongWay = 2,
    // DisqualifyPenaltyEnteringPitsUnderRed = 3,
    // DisqualifyPenaltyExitingPitsUnderRed = 4,
    // DisqualifyPenaltyFailedDriverChange = 5,
    // DisqualifyPenaltyThreeDriveThroughsInLap = 6,
    // DisqualifyPenaltyLappedFieldMultipleTimes = 7,
    // DisqualifyPenaltyIgnoredDriveThroughPenalty = 8,
    // DisqualifyPenaltyIgnoredStopAndGoPenalty = 9,
    // DisqualifyPenaltyIgnoredPitStopPenalty = 10,
    // DisqualifyPenaltyIgnoredTimePenalty = 11,
    // DisqualifyPenaltyExcessiveCutting = 12,
    // DisqualifyPenaltyIgnoredBlueFlag = 13,
    // DisqualifyPenaltyMax = 14
    public int PenaltyReason;

    // -1 unavailable, 0 = ignition off, 1 = ignition on but not running, 2 = ignition on and running
    public int EngineState;

    // Reserved data
    public int Unused1;
    public float Unused2;
    public float Unused3;
}
