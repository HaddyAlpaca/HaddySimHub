using HaddySimHub.Interfaces;
using HaddySimHub.Models;

namespace HaddySimHub.Displays.Msfs;

/// <summary>
/// Emits a looping sample flight so the flight dashboard can be developed without
/// Microsoft Flight Simulator running. Values are deterministic rather than random:
/// a jittering artificial horizon is useless for judging the layout.
/// </summary>
public class TestDisplay : TestDisplayBase
{
    /// <summary>
    /// Waypoints of the sample flight plan. The last one is the destination.
    /// </summary>
    private static readonly string[] Waypoints = ["SONEB", "REDFA", "ARTIP", "EHAM"];

    /// <summary>
    /// Ground distance covered per tick. Deliberately far higher than the 312 kt
    /// ground speed would give in half a second, so the navigation panel visibly
    /// counts down instead of looking frozen.
    /// </summary>
    private const float DemoNmPerTick = 0.35f;

    private const float GroundSpeedKts = 312f;
    private const float FuelFlowPph = 640f;
    private const float FuelCapacityLbs = 1200f;

    /// <summary>Length of the configuration loop, in ticks: cruise, then an approach.</summary>
    private const int ConfigCycleTicks = 300;

    /// <summary>Tick within the configuration loop where the approach configuration starts.</summary>
    private const int ApproachStartTick = 240;

    private int _tick;
    private int _waypointIndex;
    private float _distanceToWaypointNm = 22f;
    private float _fuelLbs = FuelCapacityLbs;

    public TestDisplay(
        string id,
        IDataConverter<DisplayUpdate, DisplayUpdate> identityDataConverter,
        IDisplayUpdateSender displayUpdateSender)
        : base(id, identityDataConverter, displayUpdateSender)
    {
    }

    protected override DisplayUpdate GenerateDisplayUpdate()
    {
        _tick++;

        AdvanceFlightPlan();
        BurnFuel();

        var phase = _tick * 0.05;
        var pitch = (float)(Math.Sin(phase) * 4);
        var bank = (float)(Math.Sin(phase * 0.4) * 15);
        var heading = (float)((_tick * 0.4) % 360);

        var configTick = _tick % ConfigCycleTicks;
        var onApproach = configTick >= ApproachStartTick;

        var remainingLegs = Waypoints.Length - 1 - _waypointIndex;
        var distanceToDestination = _distanceToWaypointNm + (remainingLegs * 42f);

        return new DisplayUpdate
        {
            Type = DisplayType.FlightDashboard,
            Data = new FlightData
            {
                // Speed
                IndicatedAirspeed = (float)(268 + Math.Sin(phase * 0.7) * 6),
                TrueAirspeed = (float)(318 + Math.Sin(phase * 0.7) * 6),
                GroundSpeed = GroundSpeedKts,
                MachNumber = 0.62f,
                StallWarning = false,
                OverspeedWarning = false,

                // Attitude
                PitchDegrees = pitch,
                BankDegrees = bank,
                SlipBall = (float)(Math.Sin(phase * 0.9) * 0.25),
                TurnRate = bank / 10f,

                // Altitude
                IndicatedAltitude = (float)(24000 + Math.Sin(phase) * 600),
                AltitudeAboveGround = (float)(23860 + Math.Sin(phase) * 600),
                VerticalSpeed = pitch * 180f,
                AltimeterSettingHpa = 1013f,
                OnGround = false,

                // Heading
                HeadingMagnetic = heading,
                HeadingTrue = (heading + 2) % 360,
                GroundTrack = (heading + 3) % 360,
                WindDirection = 270f,
                WindSpeed = 34f,

                // Autopilot
                AutopilotMaster = true,
                HeadingHold = !onApproach,
                HeadingBug = (heading + 12) % 360,
                AltitudeHold = true,
                AltitudeTarget = 24000f,
                SpeedHold = true,
                SpeedTarget = 270f,
                VerticalSpeedTarget = 0f,
                NavHold = !onApproach,
                ApproachHold = onApproach,

                // Navigation
                HasActiveFlightPlan = true,
                NextWaypointId = Waypoints[_waypointIndex],
                DistanceToWaypointNm = _distanceToWaypointNm,
                WaypointEteSeconds = EteSeconds(_distanceToWaypointNm),
                DestinationId = Waypoints[^1],
                DistanceToDestinationNm = distanceToDestination,
                DestinationEteSeconds = EteSeconds(distanceToDestination),
                DestinationEtaUtcSeconds = 13 * 3600 + 38 * 60,
                CrossTrackErrorNm = (float)(Math.Sin(phase * 0.3) * 0.4),

                // Engine
                EngineCount = 2,
                EngineType = EngineType.Jet,
                EnginePrimaryPct = (float)(78 + Math.Sin(phase * 0.6) * 4),
                EngineRpm = null,
                FuelFlowPph = FuelFlowPph,
                OilTemperature = 92.4f,
                OilPressure = 45.2f,
                ManifoldPressure = null,

                // Fuel
                FuelQuantityLbs = _fuelLbs,
                FuelCapacityLbs = FuelCapacityLbs,
                FuelEnduranceSeconds = (int)(_fuelLbs / FuelFlowPph * 3600),

                // Configuration
                FlapsHandleIndex = onApproach ? 2 : 0,
                FlapsHandlePositions = 5,
                GearPercentExtended = onApproach ? 100f : 0f,
                GearHandleDown = onApproach,
                SpoilersPct = 0f,
                SpoilersArmed = onApproach,
                ParkingBrakeOn = false,
                ElevatorTrimPct = 4f,

                // Lights
                LandingLightsOn = onApproach,
                TaxiLightsOn = false,
                StrobeLightsOn = true,
                NavLightsOn = true,
                BeaconOn = true,

                // Miscellaneous
                AircraftTitle = "Cessna Citation Longitude",
                SimTimeUtcSeconds = (12 * 3600 + 4 * 60 + (_tick / 2)) % 86400,
            },
        };
    }

    private void AdvanceFlightPlan()
    {
        _distanceToWaypointNm -= DemoNmPerTick;
        if (_distanceToWaypointNm > 0)
        {
            return;
        }

        // Sequence to the next leg, wrapping back to the start so the demo loops.
        _waypointIndex = (_waypointIndex + 1) % Waypoints.Length;
        _distanceToWaypointNm = 22f + (_waypointIndex * 6f);
    }

    private void BurnFuel()
    {
        _fuelLbs -= 0.9f;

        // Refill once the low-fuel warning has had its moment, so the loop keeps running.
        if (_fuelLbs < 180f)
        {
            _fuelLbs = FuelCapacityLbs;
        }
    }

    private static int EteSeconds(float distanceNm) => (int)(distanceNm / GroundSpeedKts * 3600);
}
