using System.Runtime.InteropServices;

namespace HaddySimHub.Displays.AC;

/// <summary>
/// Assetto Corsa telemetry data from shared memory
/// Based on AC shared memory structure
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct ACTelemetry
{
    // Physics
    public float SteerAngle;           // steering input in radians
    public float ThrottleInput;        // 0.0-1.0
    public float BrakeInput;           // 0.0-1.0
    public float ClutchInput;          // 0.0-1.0
    public float LambdaCoeff;          // air/fuel lambda
    public float Rpm;                  // engine rpm
    public float MaxRpm;               // max rpm
    public float MaxTurbo;             // max turbo level
    public float Gear;                 // current gear (0 = N, 1 = 1st, -1 = R)
    public float CarDamage;            // 0.0-1.0
    public float NumberOfTyresOut;     // number of tyres outside track
    public float PitLimiterOn;         // 0 or 1
    public float Abs;                  // 0.0-1.0 or -1 if not available
    public float TcInAction;           // traction control in action 0 or 1
    public float TotalMass;            // total car mass
    public float P1TireOut;            // P1 tyre out
    public float P1TireLoad;           // P1 tyre load
    public float P1TireTemp;           // P1 tyre temperature
    public float P1TireWear;           // P1 tyre wear 0.0-1.0
    public float P2TireOut;
    public float P2TireLoad;
    public float P2TireTemp;
    public float P2TireWear;
    public float P3TireOut;
    public float P3TireLoad;
    public float P3TireTemp;
    public float P3TireWear;
    public float P4TireOut;
    public float P4TireLoad;
    public float P4TireTemp;
    public float P4TireWear;
    public float SpeedMps;             // speed in m/s
    public float AccG;                 // acceleration in G
    public float SlipAngleFront;       // front slip angle
    public float SlipAngleRear;        // rear slip angle
    public float SlipRatioFront;       // front slip ratio
    public float SlipRatioRear;        // rear slip ratio
    public float NormalizedSplinePosTrack;  // track position
    public float CarAcceleration;      // car acceleration m/s^2
    public float NumberOfContactPoints;
    public float DrsEnabled;           // DRS enabled
    public float DrsState;             // DRS state
    public float LocalAngularVelocity; // angular velocity

    // Kerb and track
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
    public float[] LocalAngularVelocityVector;

    public float FinalFF;              // final force feedback
    public float PerformanceMeter;     // performance meter
    public float EngineBrake;          // engine brake
    public float ErsRecoveryLevel;     // ERS recovery level
    public float ErsDeployLevel;       // ERS deploy level
    public float ersHarvestedThisLapMGUK;  // ERS harvested MGU-K
    public float ersHarvestedThisLapMGUH;  // ERS harvested MGU-H
    public float FuelEstimatedLaps;    // fuel estimated laps
    public float AirDensity;           // air density
    public float AirTemp;              // air temperature
    public float RoadTemp;             // road temperature
    public float FuelPressure;         // fuel pressure
    public float RainLights;           // rain lights
    public float RainTyres;            // rain tyres

    // Session and track
    public int SessionIndex;           // session index
    public int SessionType;            // 0 = practice, 1 = qualify, 2 = race
    public int CurrentLapInvalid;      // current lap invalid
    public int CurrentLap;             // current lap
    public int TotalLaps;              // total laps
    public int LastLapTime;            // last lap time in ms
    public int CurrentLapTime;         // current lap time in ms
    public int DeltaLapTime;           // delta to best lap in ms
    public int ICurrentTime;           // current track time in ms
    public int IAbs;                   // ABS level 0-1
    public int ITc;                    // TC level 0-1
    public int SessionTimeLeft;        // session time left in ms
    public int IsEngineRunning;        // engine running 0 or 1
}
