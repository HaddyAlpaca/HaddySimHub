using System.Runtime.InteropServices;

namespace HaddySimHub.Displays.ACC;

/// <summary>
/// Assetto Corsa Competizione telemetry data from shared memory
/// Based on ACC shared memory structure
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct ACCTelemetry
{
    // Physics
    public float SteerAngle;           // steering input in radians
    public float ThrottleInput;        // 0.0-1.0
    public float BrakeInput;           // 0.0-1.0
    public float ClutchInput;          // 0.0-1.0
    public float Rpm;                  // engine rpm
    public float MaxRpm;               // max rpm
    public float Gear;                 // current gear (0 = N, 1 = 1st, -1 = R)
    public float CarDamage;            // car damage 0.0-1.0
    public float PitLimiterOn;         // pit limiter on/off
    public float Abs;                  // ABS level 0.0-1.0
    public float TcInAction;           // TC acting 0 or 1
    public float EngineMap;            // engine map
    public float FuelAutoConsumption;  // fuel consumption per lap
    public float RideHeight;           // ride height
    public float FuelEstimatedLaps;    // fuel estimated laps

    // Tyres
    public float TireCoreTemp1;        // Front Left
    public float TireCoreTemp2;        // Front Right
    public float TireCoreTemp3;        // Rear Left
    public float TireCoreTemp4;        // Rear Right
    public float BrakeTemp1;           // Front Left
    public float BrakeTemp2;           // Front Right
    public float BrakeTemp3;           // Rear Left
    public float BrakeTemp4;           // Rear Right
    public float TireLoad1;            // Front Left load
    public float TireLoad2;            // Front Right load
    public float TireLoad3;            // Rear Left load
    public float TireLoad4;            // Rear Right load
    public float TireWear1;            // Front Left wear 0.0-1.0
    public float TireWear2;            // Front Right wear 0.0-1.0
    public float TireWear3;            // Rear Left wear 0.0-1.0
    public float TireWear4;            // Rear Right wear 0.0-1.0
    public float TirePressure1;        // Front Left
    public float TirePressure2;        // Front Right
    public float TirePressure3;        // Rear Left
    public float TirePressure4;        // Rear Right

    // Speed and motion
    public float SpeedMs;              // speed m/s
    public float AccGs;                // acceleration G
    public float LocalAngularVelocity; // angular velocity
    public float SlipAngleFront;       // slip angle front
    public float SlipAngleRear;        // slip angle rear
    public float PerformanceMeter;     // performance meter
    public float AirDensity;           // air density
    public float AirTemp;              // air temperature C
    public float RoadTemp;             // road temperature C
    public float FuelPressure;         // fuel pressure bar
    public float CurrentLapInvalid;    // lap invalid
    public float LapTimeMs;            // lap time ms
    public float DeltaMs;              // delta to best lap ms
    public float SessionTimeLeftMs;    // session time left ms

    // Session info
    public int SessionIndex;           // session index
    public int SessionType;            // 0 = practice, 1 = qualify, 2 = race
    public int CurrentLapCount;        // current lap number
    public int MaxLaps;                // max laps in race
    public int MaxSessionLaps;         // max session laps
    public int IsEngineRunning;        // engine running
}
