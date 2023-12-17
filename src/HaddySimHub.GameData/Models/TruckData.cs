using System;

namespace HaddySimHub.GameData.Models;

public struct TruckData
{
    public string DestinationCity { get; init; }
    public string DestinationCompany { get; init; }
    public string SourceCity { get; init; }
    public string SourceCompany { get; set; }
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
    public ulong JobIncome { get; init; }
    public string JobCargoName { get; init; }
    public int JobCargoMass { get; init; }
    public int JobCargoDamage { get; init; }

    /* Damage */
    public int DamageCabin { get; init; }
    public int DamageTransmission { get; init; }
    public int DamageWheels { get; init; }
    public int DamageEngine { get; init; }
    public int DamageChassis { get; init; }
    public int DamageTrailer { get; init; }
    public bool TrailerAttached { get; init; }
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
    public bool HazardLightsOn { get; init; }
    public bool EngineWaterTempWarningOn { get; init; }
    public bool OilPressureWarningOn { get; init; }
    public bool FuelWarningOn { get; init; }
    public bool BlinkerLeftOn { get; init; }
    public bool BlinkerRightOn { get; init; }

    /// <summary>
    /// Description of the current truck
    /// </summary>
    public string TruckName { get; init; }
}