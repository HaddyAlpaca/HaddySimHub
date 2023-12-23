﻿using System;

namespace HaddySimHub.GameData.Models;

public struct TruckData
{
    public string DestinationCity { get; init; }

    public string DestinationCompany { get; init; }

    public string SourceCity { get; init; }

    public string SourceCompany { get; set; }

    /// <summary>
    /// Gets distance remaining (meters).
    /// </summary>
    public int DistanceRemaining { get; init; }

    /// <summary>
    /// Gets time remaining (minutes) to complete the distance.
    /// </summary>
    public int TimeRemaining { get; init; }

    /// <summary>
    /// Gets or sets time remaining (minutes) to complete distance in real life.
    /// </summary>
    public int TimeRemainingIrl { get; set; }

    /*    Current time                      */
    public DateTime Time { get; init; }

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
    public int FuelDistance { get; init; }

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

    public string JobCargoName { get; init; }

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
    public bool BatteryWarningOn { get; init; }

    public bool HazardLightsOn { get; init; }

    public bool EngineWaterTempWarningOn { get; init; }

    public bool OilPressureWarningOn { get; init; }

    public bool FuelWarningOn { get; init; }

    public bool BlinkerLeftOn { get; init; }

    public bool BlinkerRightOn { get; init; }

    /// <summary>
    /// Gets description of the current truck.
    /// </summary>
    public string TruckName { get; init; }

    public int NumberOfTrailersAttached { get; init; }

    public bool WipersOn { get; init; }
}