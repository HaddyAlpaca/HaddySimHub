namespace HaddySimHub.Models;

public sealed record TruckData
{
    public string DestinationCity { get; init; } = string.Empty;

    public string DestinationCompany { get; init; } = string.Empty;

    public string SourceCity { get; init; } = string.Empty;

    public string SourceCompany { get; set; } = string.Empty;

    /// <summary>
    /// Gets distance remaining (meters).
    /// </summary>
    public float DistanceRemaining { get; init; }

    /// <summary>
    /// Gets time remaining (minutes) to complete the distance.
    /// </summary>
    public int TimeRemaining { get; init; }

    /// <summary>
    /// Gets or sets time remaining (minutes) to complete distance in real life.
    /// </summary>
    public int TimeRemainingIrl { get; set; }

    /*    Current time                      */
    public ulong GameTime { get; init; }

    /*    Critical stopping information     */

    /// <summary>
    /// Gets time remaining (minutes) to the next rest stop.
    /// </summary>
    public int RestTimeRemaining { get; init; }

    /// <summary>
    /// Gets time remaining (minutes) to the next rest stop in real time.
    /// </summary>
    public int RestTimeRemainingIrl { get; init; }

    /// <summary>
    /// Gets percentage of fuel remaining.
    /// </summary>
    /// <summary>
    /// Distance (km) that can be travelled with the current fuel level.
    /// </summary>
    public float FuelDistance { get; init; }

    /// <summary>
    /// Amount of fuel in liters
    /// </summary>
    public float FuelAmount { get; init; }

    /// <summary>
    /// Amount of AdBlue in liters
    /// </summary>
    public float AdBlueAmount { get; init; }

    public bool AdBlueWarningOn { get; init; }

    /*  Job data    */

    /// <summary>
    /// Gets time remaining (minutes) to complete the current job.
    /// </summary>
    public long JobTimeRemaining { get; init; }

    /// <summary>
    /// Gets time remaining (minutes) to complete the current job in real life.
    /// </summary>
    public long JobTimeRemainingIrl { get; init; }

    /// <summary>
    /// Gets income for the current job.
    /// </summary>
    public ulong JobIncome { get; init; }

    public string JobCargoName { get; init; } = string.Empty;

    public int JobCargoMass { get; init; }

    public int JobCargoDamage { get; init; }

    /* Damage */
    public int DamageTruckCabin { get; init; }

    public int DamageTruckTransmission { get; init; }

    public int DamageTruckWheels { get; init; }

    public int DamageTruckEngine { get; init; }

    public int DamageTruckChassis { get; init; }

    public int DamageTrailerChassis { get; init; }

    public int DamageTrailerCargo { get; init; }

    public int DamageTrailerWheels { get; init; }

    public int DamageTrailerBody { get; init; }
    /*  Dashboard data    */

    /// <summary>
    /// Gets speed (km/h).
    /// </summary>
    public short Speed { get; init; }

    /// <summary>
    /// Gets speed limit (km/h).
    /// </summary>
    public short SpeedLimit { get; init; }

    /// <summary>
    /// Gets engine revs in RPM.
    /// </summary>
    public int Rpm { get; init; }

    /// <summary>
    /// Gets maximum engine revs in RPM.
    /// </summary>
    public int RpmMax { get; init; }

    /// <summary>
    /// Gets selected gear.
    /// </summary>
    public short Gear { get; init; }

    /// <summary>
    /// Gets a value indicating whether cruise control is active.
    /// </summary>
    public bool CruiseControlOn { get; init; }

    /// <summary>
    /// Gets cruise control speed (km/h).
    /// </summary>
    public short CruiseControlSpeed { get; init; }

    /// <summary>
    /// Gets a value indicating whether low beam active.
    /// </summary>
    public bool LowBeamOn { get; init; }

    /// <summary>
    /// Gets a value indicating whether high beam active.
    /// </summary>
    public bool HighBeamOn { get; init; }

    /// <summary>
    /// Gets a value indicating whether parking brake active.
    /// </summary>
    public bool ParkingBrakeOn { get; init; }

    /// <summary>
    /// Gets a value indicating whether battery warning active.
    /// </summary>
    public bool BatteryVoltageWarningOn { get; init; }

    public float BatteryVoltage { get; init; }

    public bool HazardLightsOn { get; init; }

    public bool OilPressureWarningOn { get; init; }

    public bool FuelWarningOn { get; init; }

    public bool BlinkerLeftOn { get; init; }

    public bool BlinkerRightOn { get; init; }

    /// <summary>
    /// Gets description of the current truck.
    /// </summary>
    public string TruckName { get; init; } = string.Empty;

    public int NumberOfTrailersAttached { get; init; }

    public bool WipersOn { get; init; }

    /// <summary>
    /// Average consumption of the fuel in liters/ 100 km
    /// </summary>
    public float FuelAverageConsumption { get; init; }

    /// <summary>
    /// Throttle percentage
    /// </summary>
    public int Throttle { get; init; }


    /// <summary>
    /// Differential lock enabled?
    /// </summary>
    public bool DifferentialLock { get; init; }


    /// <summary>
    /// Oil pressure in PSI
    /// </summary>
    public float OilPressure { get; init; }

    /// <summary>
    /// Oil temp in degrees Celcius
    /// </summary>
    public float OilTemp { get; init; }

    /// <summary>
    /// Water temp in degrees Celcius
    /// </summary>
    public float WaterTemp { get; init; }

    public bool WaterTempWarningOn { get; init; }

    public string[] Messages { get; init; } = [];
}