using System;

namespace HaddySimHub.Telemetry.Models;

public enum GearRange
{
    Low,
    High
}

public struct TruckData
{
    /*  Navigation data */

    /// <summary>
    /// Description of the destination
    /// </summary>
    public string Destination { get; init; }
    /// <summary>
    /// Distance remaining (meters)
    /// </summary>
    public int DistanceRemaining { get; init; }
    /// <summary>
    /// Time remaining (minutes) to complete the distance
    /// </summary>
    public int TimeRemaining { get; init; }
    /// <summary>
    /// Time remaining (minutes) to complete distance in real life
    /// </summary>
    public int TimeRemainingIrl { get; set; }

    /*    Current time                      */
    public DateTime Time { get; init; }

    /*    Critical stopping information     */
    /// <summary>
    /// Time remaining (minutes) to the next rest stop
    /// </summary>
    public int RestTimeRemaining { get; init; }
    /// <summary>
    /// Time remaining (minutes) to the next rest stop in real time
    /// </summary>
    public int RestTimeRemainingIrl { get; init; }
    /// <summary>
    /// Percentage of fuel remaining
    /// </summary>
    public int FuelPercentage { get; init; }
    /// <summary>
    /// Distance (km) that can be travelled with the current fuel level
    /// </summary>
    public int FuelDistance { get; init; }

    /*  Job data    */

    /// <summary>
    /// Time remaining (minutes) to complete the current job
    /// </summary>
    public long JobTimeRemaining { get; init; }
    /// <summary>
    /// Time remaining (minutes) to complete the current job in real life
    /// </summary>
    public long JobTimeRemainingIrl { get; init; }
    /// <summary>
    /// Income for the current job
    /// </summary>
    public int JobIncome { get; init; }

    /*  Dashboard data    */

    /// <summary>
    /// Speed (km/h)
    /// </summary>
    public short Speed { get; init; }
    /// <summary>
    /// Speed limit (km/h)
    /// </summary>
    public short SpeedLimit { get; init; }
    /// <summary>
    /// Engine revs in RPM
    /// </summary>
    public int Rpm { get; init; }
    /// <summary>
    /// Maximum engine revs in RPM
    /// </summary>
    public int RpmMax { get; init; }
    /// <summary>
    /// Selected gear
    /// </summary>
    public short Gear { get; init; }
    /// <summary>
    /// Selected gear range
    /// </summary>
    public GearRange GearRange { get; init; }
    /// <summary>
    /// Cruise control is active
    /// </summary>
    public bool CruiseControlOn { get; init; }
    /// <summary>
    /// Cruise control speed (km/h)
    /// </summary>
    public short CruiseControlSpeed { get; init; }
    /// <summary>
    /// Low beam active
    /// </summary>
    public bool LowBeamOn { get; init; }
    /// <summary>
    /// High beam active
    /// </summary>
    public bool HighBeamOn { get; init; }
    /// <summary>
    /// Parking brake active
    /// </summary>
    public bool ParkingBrakeOn { get; init; }
    /// <summary>
    /// Battery warning active
    /// </summary>
    public bool BatteryWarningOn { get; init; }
}