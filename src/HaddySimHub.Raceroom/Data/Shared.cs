using System.Runtime.InteropServices;

namespace HaddySimHub.Raceroom.Data;


[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct Shared
{
    //////////////////////////////////////////////////////////////////////////
    // Version
    //////////////////////////////////////////////////////////////////////////
    public int VersionMajor;
    public int VersionMinor;
    public int AllDriversOffset; // Offset to NumCars variable
    public int DriverDataSize; // Size of DriverData

    //////////////////////////////////////////////////////////////////////////
    // Game State
    //////////////////////////////////////////////////////////////////////////

    public int GamePaused;
    public int GameInMenus;
    public int GameInReplay;
    public int GameUsingVr;

    public int GameUnused1;

    //////////////////////////////////////////////////////////////////////////
    // High Detail
    //////////////////////////////////////////////////////////////////////////

    // High precision data for player's vehicle only
    public PlayerData Player;

    //////////////////////////////////////////////////////////////////////////
    // Event And Session
    //////////////////////////////////////////////////////////////////////////

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
    public byte[] TrackName; // UTF-8
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
    public byte[] LayoutName; // UTF-8

    public int TrackId;
    public int LayoutId;

    // Layout length in meters
    public float LayoutLength;
    public SectorStarts<float> SectorStartFactors;

    // Race session durations
    // Note: Index 0-2 = race 1-3
    // Note: Value -1 = N/A
    // Note: If both laps and minutes are more than 0, race session starts with minutes then adds laps
    public RaceDuration<int> RaceSessionLaps;
    public RaceDuration<int> RaceSessionMinutes;

    // The current race event index, for championships with multiple events
    // Note: 0-indexed, -1 = N/A
    public int EventIndex;

    // Which session the player is in (practice, qualifying, race, etc.)
    // Note: See the R3E.Constant.Session enum
    public int SessionType;

    // The current iteration of the current type of session (second qualifying session, etc.)
    // Note: 1 = first, 2 = second etc, -1 = N/A
    public int SessionIteration;

    // If the session is time based, lap based or time based with an extra lap at the end
    public int SessionLengthFormat;

    // Unit: Meter per second (m/s)
    public float SessionPitSpeedLimit;

    // Which phase the current session is in (gridwalk, countdown, green flag, etc.)
    // Note: See the R3E.Constant.SessionPhase enum
    public int SessionPhase;

    // Which phase start lights are in; -1 = unavailable, 0 = off, 1-5 = redlight on and counting down, 6 = greenlight on
    // Note: See the r3e_session_phase enum
    public int StartLights;

    // -1 = no data available
    //  0 = not active
    //  1 = active
    //  2 = 2x
    //  3 = 3x
    //  4 = 4x
    public int TireWearActive;

    // -1 = no data
    //  0 = not active
    //  1 = active
    //  2 = 2x
    //  3 = 3x
    //  4 = 4x
    public int FuelUseActive;

    // Total number of laps in the race, or -1 if player is not in race mode (practice, test mode, etc.)
    public int NumberOfLaps;

    // Amount of time and time remaining for the current session
    // Note: Only available in time-based sessions, -1.0 = N/A
    // Units: Seconds
    public float SessionTimeDuration;
    public float SessionTimeRemaining;

    // Server max incident points, -1 = N/A
    public int MaxIncidentPoints;

    // Reserved data
    public float EventUnused2;

    //////////////////////////////////////////////////////////////////////////
    // Pit
    //////////////////////////////////////////////////////////////////////////

    // Current status of the pit stop
    // Note: See the R3E.Constant.PitWindow enum
    public int PitWindowStatus;

    // The minute/lap from which you're obligated to pit (-1 = N/A)
    // Unit: Minutes in time-based sessions, otherwise lap
    public int PitWindowStart;

    // The minute/lap into which you need to have pitted (-1 = N/A)
    // Unit: Minutes in time-based sessions, otherwise lap
    public int PitWindowEnd;

    // If current vehicle is in pitline (-1 = N/A)
    public int InPitlane;

    // What is currently selected in pit menu, and array of states (preset/buttons: -1 = not selectable, 1 = selectable) (actions: -1 = N/A, 0 = unmarked for fix, 1 = marked for fix)
    public int PitMenuSelection;
    public PitMenuState PitMenuState;

    // Current vehicle pit state (-1 = N/A, 0 = None, 1 = Requested stop, 2 = Entered pitlane heading for pitspot, 3 = Stopped at pitspot, 4 = Exiting pitspot heading for pit exit)
    public int PitState;

    // Current vehicle pitstop actions duration
    public float PitTotalDuration;
    public float PitElapsedTime;

    // Current vehicle pit action (-1 = N/A, 0 = None, 1 = Preparing, (combination of 2 = Penalty serve, 4 = Driver change, 8 = Refueling, 16 = Front tires, 32 = Rear tires, 64 = Body, 128 = Front wing, 256 = Rear wing, 512 = Suspension))
    public int PitAction;

    // Number of pitstops the current vehicle has performed (-1 = N/A)
    public int NumPitstopsPerformed;

    public float PitMinDurationTotal;
    public float PitMinDurationLeft;

    //////////////////////////////////////////////////////////////////////////
    // Scoring & Timings
    //////////////////////////////////////////////////////////////////////////

    // The current state of each type of flag
    public Flags Flags;

    // Current position (1 = first place)
    public int Position;
    // Based on performance index
    public int PositionClass;

    // Note: See the R3E.Constant.FinishStatus enum
    public int FinishStatus;

    // Total number of cut track warnings (-1 = N/A)
    public int CutTrackWarnings;

    // The number of penalties the car currently has pending of each type (-1 = N/A)
    public CutTrackPenalties Penalties;
    // Total number of penalties pending for the car
    // Note: See the 'penalties' field
    public int NumPenalties;

    // How many laps the player has completed. If this value is 6, the player is on his 7th lap. -1 = n/a
    public int CompletedLaps;
    public int CurrentLapValid;
    public int TrackSector;
    public float LapDistance;
    // fraction of lap completed, 0.0-1.0, -1.0 = N/A
    public float LapDistanceFraction;

    // The current best lap time for the leader of the session (-1.0 = N/A)
    public float LapTimeBestLeader;
    // The current best lap time for the leader of the player's class in the current session (-1.0 = N/A)
    public float LapTimeBestLeaderClass;
    // Sector times of fastest lap by anyone in session
    // Unit: Seconds (-1.0 = N/A)
    public Sectors<float> SectorTimesSessionBestLap;
    // Unit: Seconds (-1.0 = none)
    public float LapTimeBestSelf;
    public Sectors<float> SectorTimesBestSelf;
    // Unit: Seconds (-1.0 = none)
    public float LapTimePreviousSelf;
    public Sectors<float> SectorTimesPreviousSelf;
    // Unit: Seconds (-1.0 = none)
    public float LapTimeCurrentSelf;
    public Sectors<float> SectorTimesCurrentSelf;
    // The time delta between the player's time and the leader of the current session (-1.0 = N/A)
    public float LapTimeDeltaLeader;
    // The time delta between the player's time and the leader of the player's class in the current session (-1.0 = N/A)
    public float LapTimeDeltaLeaderClass;
    // Time delta between the player and the car placed in front (-1.0 = N/A)
    // Units: Seconds
    public float TimeDeltaFront;
    // Time delta between the player and the car placed behind (-1.0 = N/A)
    // Units: Seconds
    public float TimeDeltaBehind;
    // Time delta between this car's current laptime and this car's best laptime
    // Unit: Seconds (-1000.0 = N/A)
    public float TimeDeltaBestSelf;
    // Best time for each individual sector no matter lap
    // Unit: Seconds (-1.0 = N/A)
    public Sectors<float> BestIndividualSectorTimeSelf;
    public Sectors<float> BestIndividualSectorTimeLeader;
    public Sectors<float> BestIndividualSectorTimeLeaderClass;
    public int IncidentPoints;

    // -1 = N/A, 0 = this and next lap valid, 1 = this lap invalid, 2 = this and next lap invalid
    public int LapValidState;

    // Reserved data
    public float ScoreUnused1;
    public float ScoreUnused2;

    //////////////////////////////////////////////////////////////////////////
    // Vehicle information
    //////////////////////////////////////////////////////////////////////////

    public DriverInfo VehicleInfo;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
    public byte[] PlayerName; // UTF-8

    //////////////////////////////////////////////////////////////////////////
    // Vehicle State
    //////////////////////////////////////////////////////////////////////////

    // Which controller is currently controlling the player's car (AI, player, remote, etc.)
    // Note: See the R3E.Constant.Control enum
    public int ControlType;

    // Unit: Meter per second (m/s)
    public float CarSpeed;

    // Unit: Radians per second (rad/s)
    public float EngineRps;
    public float MaxEngineRps;
    public float UpshiftRps;

    // -2 = N/A, -1 = reverse, 0 = neutral, 1 = first gear, ...
    public int Gear;
    // -1 = N/A
    public int NumGears;

    // Physical location of car's center of gravity in world space (X, Y, Z) (Y = up)
    public Vector3<float> CarCgLocation;
    // Pitch, yaw, roll
    // Unit: Radians (rad)
    public Orientation<float> CarOrientation;
    // Acceleration in three axes (X, Y, Z) of car body in local-space.
    // From car center, +X=left, +Y=up, +Z=back.
    // Unit: Meter per second squared (m/s^2)
    public Vector3<float> LocalAcceleration;

    // Unit: Kilograms (kg)
    // Note: Car + penalty weight + fuel
    public float TotalMass;
    // Unit: Liters (l)
    // Note: Fuel per lap show estimation when not enough data, then max recorded fuel per lap
    // Note: Not valid for remote players
    public float FuelLeft;
    public float FuelCapacity;
    public float FuelPerLap;
    // Unit: Celsius (C)
    // Note: Not valid for AI or remote players
    public float EngineWaterTemp;
    public float EngineOilTemp;
    // Unit: Kilopascals (KPa)
    // Note: Not valid for AI or remote players
    public float FuelPressure;
    // Unit: Kilopascals (KPa)
    // Note: Not valid for AI or remote players
    public float EngineOilPressure;

    // Unit: (Bar)
    // Note: Not valid for AI or remote players (-1.0 = N/A)
    public float TurboPressure;

    // How pressed the throttle pedal is
    // Range: 0.0 - 1.0 (-1.0 = N/A)
    // Note: Not valid for AI or remote players
    public float Throttle;
    public float ThrottleRaw;
    // How pressed the brake pedal is
    // Range: 0.0 - 1.0 (-1.0 = N/A)
    // Note: Not valid for AI or remote players
    public float Brake;
    public float BrakeRaw;
    // How pressed the clutch pedal is
    // Range: 0.0 - 1.0 (-1.0 = N/A)
    // Note: Not valid for AI or remote players
    public float Clutch;
    public float ClutchRaw;
    // How much the steering wheel is turned
    // Range: -1.0 - 1.0
    // Note: Not valid for AI or remote players
    public float SteerInputRaw;
    // How many degrees in steer lock (center to full lock)
    // Note: Not valid for AI or remote players
    public int SteerLockDegrees;
    // How many degrees in wheel range (degrees full left to rull right)
    // Note: Not valid for AI or remote players
    public int SteerWheelRangeDegrees;

    // Aid settings
    public AidSettings AidSettings;

    // DRS data
    public DRS Drs;

    // Pit limiter (-1 = N/A, 0 = inactive, 1 = active)
    public int PitLimiter;

    // Push to pass data
    public PushToPass PushToPass;

    // How much the vehicle's brakes are biased towards the back wheels (0.3 = 30%, etc.) (-1.0 = N/A)
    // Note: Not valid for AI or remote players
    public float BrakeBias;

    // DRS activations available in total (-1 = N/A or endless)
    public int DrsNumActivationsTotal;
    // PTP activations available in total (-1 = N/A, or there's no restriction per lap, or endless)
    public int PtpNumActivationsTotal;

    // Battery state of charge
    // Range: 0.0 - 100.0 (-1.0 = N/A)
    public float BatterySoC;

    // Brake water tank (-1.0 = N/A)
    // Unit: Liters (l)
    public float WaterLeft;

    // Reserved data
    Orientation<float> VehicleUnused1;

    //////////////////////////////////////////////////////////////////////////
    // Tires
    //////////////////////////////////////////////////////////////////////////

    // Which type of tires the player's car has (option, prime, etc.)
    // Note: See the R3E.Constant.TireType enum, deprecated - use the values further down instead
    public int TireType;

    // Rotation speed
    // Uint: Radians per second
    public TireData<float> TireRps;
    // Wheel speed
    // Uint: Meters per second
    public TireData<float> TireSpeed;
    // Range: 0.0 - 1.0 (-1.0 = N/A)
    public TireData<float> TireGrip;
    // Range: 0.0 - 1.0 (-1.0 = N/A)
    public TireData<float> TireWear;
    // (-1 = N/A, 0 = false, 1 = true)
    public TireData<int> TireFlatspot;
    // Unit: Kilopascals (KPa) (-1.0 = N/A)
    // Note: Not valid for AI or remote players
    public TireData<float> TirePressure;
    // Percentage of dirt on tire (-1.0 = N/A)
    // Range: 0.0 - 1.0
    public TireData<float> TireDirt;

    // Current temperature of three points across the tread of the tire (-1.0 = N/A)
    // Optimum temperature
    // Cold temperature
    // Hot temperature
    // Unit: Celsius (C)
    // Note: Not valid for AI or remote players
    public TireData<TireTempInformation> TireTemp;

    // Which type of tires the car has (option, prime, etc.)
    // Note: See the R3E.Constant.TireType enum
    public int TireTypeFront;
    public int TireTypeRear;
    // Which subtype of tires the car has
    // Note: See the R3E.Constant.TireSubtype enum
    public int TireSubtypeFront;
    public int TireSubtypeRear;

    // Current brake temperature (-1.0 = N/A)
    // Optimum temperature
    // Cold temperature
    // Hot temperature
    // Unit: Celsius (C)
    // Note: Not valid for AI or remote players
    public TireData<BrakeTemp> BrakeTemp;
    // Brake pressure (-1.0 = N/A)
    // Unit: Kilo Newtons (kN)
    // Note: Not valid for AI or remote players
    public TireData<float> BrakePressure;

    // Reserved data
    public int TractionControlSetting;
    public int EngineMapSetting;
    public int EngineBrakeSetting;

    // -1.0 = N/A, 0.0 -> 100.0 percent
    public float TractionControlPercent;

    // Which type of material under player car tires (tarmac, gravel, etc.)
    // Note: See the R3E.Constant.MtrlType enum
    public TireData<int> TireOnMtrl;

    // Tire load (N)
    // -1.0 = N/A
    public TireData<float> TireLoad;

    //////////////////////////////////////////////////////////////////////////
    // Damage
    //////////////////////////////////////////////////////////////////////////

    // The current state of various parts of the car
    // Note: Not valid for AI or remote players
    public CarDamage CarDamage;

    //////////////////////////////////////////////////////////////////////////
    // Driver Info
    //////////////////////////////////////////////////////////////////////////

    // Number of cars (including the player) in the race
    public int NumCars;

    // Contains name and basic vehicle info for all drivers in place order
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
    public DriverData[] DriverData;
}
