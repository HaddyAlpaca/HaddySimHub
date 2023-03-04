using System.Runtime.InteropServices;

namespace HaddySimHub.Raceroom.Data;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct PlayerData
{
    // Virtual physics time
    // Unit: Ticks (1 tick = 1/400th of a second)
    public int GameSimulationTicks;

    // Virtual physics time
    // Unit: Seconds
    public double GameSimulationTime;

    // Car world-space position
    public Vector3<double> Position;

    // Car world-space velocity
    // Unit: Meter per second (m/s)
    public Vector3<double> Velocity;

    // Car local-space velocity
    // Unit: Meter per second (m/s)
    public Vector3<double> LocalVelocity;

    // Car world-space acceleration
    // Unit: Meter per second squared (m/s^2)
    public Vector3<double> Acceleration;

    // Car local-space acceleration
    // Unit: Meter per second squared (m/s^2)
    public Vector3<double> LocalAcceleration;

    // Car body orientation
    // Unit: Euler angles
    public Vector3<double> Orientation;

    // Car body rotation
    public Vector3<double> Rotation;

    // Car body angular acceleration (torque divided by inertia)
    public Vector3<double> AngularAcceleration;

    // Car world-space angular velocity
    // Unit: Radians per second
    public Vector3<double> AngularVelocity;

    // Car local-space angular velocity
    // Unit: Radians per second
    public Vector3<double> LocalAngularVelocity;

    // Driver g-force local to car
    public Vector3<double> LocalGforce;

    // Total steering force coming through steering bars
    public double SteeringForce;
    public double SteeringForcePercentage;

    // Current engine torque
    public double EngineTorque;

    // Current downforce
    // Unit: Newtons (N)
    public double CurrentDownforce;

    // Currently unused
    public double Voltage;
    public double ErsLevel;
    public double PowerMguH;
    public double PowerMguK;
    public double TorqueMguK;

    // Car setup (radians, meters, meters per second)
    public TireData<double> SuspensionDeflection;
    public TireData<double> SuspensionVelocity;
    public TireData<double> Camber;
    public TireData<double> RideHeight;
    public double FrontWingHeight;
    public double FrontRollAngle;
    public double RearRollAngle;
    public double ThirdSpringSuspensionDeflectionFront;
    public double ThirdSpringSuspensionVelocityFront;
    public double ThirdSpringSuspensionDeflectionRear;
    public double ThirdSpringSuspensionVelocityRear;

    // Reserved data
    public double Unused1;
}
