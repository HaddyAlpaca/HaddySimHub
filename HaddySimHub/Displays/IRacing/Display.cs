using HaddySimHub.Models;
using HaddySimHub.Shared;
using iRacingSDK;

namespace HaddySimHub.Displays.IRacing;

internal sealed class Display() : DisplayBase<DataSample>()
{
    private int _lastPlayerLap;
    private int? _sessionNum;
    private float _lastLapStartFuel;
    private bool _lastPlayerCarInPitStall;
    private bool _lastOnPitRoad;
    private readonly List<float> _fuelStintHistory = [];
    private readonly List<float> _leaderLapTimes = [];
    private int _lastLeaderLap;

    public override void Start()
    {
        iRacing.NewData += async (data) =>
        {
            try
            {
                await this.SendUpdate(data);
            }
            catch (Exception ex)
            {
                Logger.Error($"{ex.Message}\n\n{ex.StackTrace}");
            }
        };
        iRacing.StartListening();
    }

    public override void Stop()
    {
        if (iRacing.IsConnected)
        {
            iRacing.StopListening();
        }
    }

    public override string Description => "IRacing";
    public override bool IsActive => ProcessHelper.IsProcessRunning("iracingui");

    protected override DisplayUpdate ConvertToDisplayUpdate(DataSample data)
    {
        var telemetry = data.Telemetry;

        var sessionData = data.SessionData;
        var session = sessionData.SessionInfo.Sessions.First(s => s.SessionNum == telemetry.SessionNum);

        var currentPlayerLap = telemetry.CarIdxLap[telemetry.PlayerCarIdx];

        if (_sessionNum != telemetry.SessionNum)
        {
            _sessionNum = telemetry.SessionNum;
            _lastPlayerLap = currentPlayerLap;
            _lastLapStartFuel = telemetry.FuelLevel;
            _fuelStintHistory.Clear();
            _leaderLapTimes.Clear();
            _lastLeaderLap = 1;
            // Initialize pit state tracking for the new session
            _lastPlayerCarInPitStall = telemetry.PlayerCarInPitStall;
            _lastOnPitRoad = telemetry.OnPitRoad;
        }

        // If we have just exited the pits (either left the pit stall or left pit road),
        // reset fuel usage history so we start a fresh stint history.
        if ((_lastPlayerCarInPitStall && !telemetry.PlayerCarInPitStall) || (_lastOnPitRoad && !telemetry.OnPitRoad))
        {
            _fuelStintHistory.Clear();
            // Reset the lap-start fuel baseline so the next lap's fuel calc starts from current level
            _lastLapStartFuel = telemetry.FuelLevel;
        }

        // Check if we completed a lap
        if (currentPlayerLap > _lastPlayerLap)
        {
            // Calculate fuel used in the last lap
            float fuelUsed = _lastLapStartFuel - telemetry.FuelLevel;
            // Only add valid fuel usage (positive and not from pit stops)
            if (fuelUsed > 0 && telemetry.FuelLevel <= _lastLapStartFuel)
            {
                _fuelStintHistory.Add(fuelUsed);
            }
            _lastLapStartFuel = telemetry.FuelLevel;
            
            // Update the last player lap
            _lastPlayerLap = currentPlayerLap;
        }

        // Update last-known pit states for next sample
        _lastPlayerCarInPitStall = telemetry.PlayerCarInPitStall;
        _lastOnPitRoad = telemetry.OnPitRoad;

        // Track leader's lap times
        var leaderCarIdx = telemetry.CarIdxPosition
            .Select((position, idx) => (position, idx))
            .OrderBy(x => x.position)
            .FirstOrDefault().idx;

        if (leaderCarIdx >= 0 && leaderCarIdx < telemetry.CarIdxLap.Length)
        {
            var leaderCurrentLap = telemetry.CarIdxLap[leaderCarIdx];
            if (leaderCurrentLap > _lastLeaderLap)
            {
                var leaderLastLapTime = telemetry.CarIdxLastLapTime[leaderCarIdx];
                if (leaderLastLapTime > 0)
                {
                    _leaderLapTimes.Add(leaderLastLapTime);
                    // Keep only the last 5 laps for a more recent average
                    if (_leaderLapTimes.Count > 5)
                    {
                        _leaderLapTimes.RemoveAt(0);
                    }
                }
                _lastLeaderLap = leaderCurrentLap;
            }
        }

        string CarScreenName = string.Empty;
        var playerInfo = sessionData.DriverInfo.CompetingDrivers.FirstOrDefault(d => d.CarIdx == telemetry.PlayerCarIdx);

        // Calculate estimated laps for time-limited sessions
        int estimatedLaps = session._SessionLaps;
        if (session.IsLimitedTime && !session.IsLimitedSessionLaps && _leaderLapTimes.Count > 0)
        {
            float avgLeaderLapTime = _leaderLapTimes.Average();
            estimatedLaps = (int)Math.Ceiling(telemetry.SessionTimeRemain / avgLeaderLapTime);
        }

        var displayUpdate = new RaceData
        {
            SessionType = session.SessionType,
            IsLimitedTime = session.IsLimitedTime,
            IsLimitedSessionLaps = session.IsLimitedSessionLaps,
            CurrentLap = telemetry.Lap,
            TotalLaps = session._SessionLaps,
            EstimatedLaps = estimatedLaps,
            Incidents = Math.Max(telemetry.PlayerCarDriverIncidentCount, 0),
            MaxIncidents = Math.Max(Math.Min(sessionData!.WeekendInfo.WeekendOptions._IncidentLimit, 999), 0),
            SessionTimeRemaining = (float)telemetry.SessionTimeRemain,
            Position = telemetry.PlayerCarPosition,
            StrengthOfField = telemetry.RaceCars.Count() > 1 ? (int)Math.Round(telemetry.RaceCars.Average(r => r.Details.Driver.IRating)) : 0,
            CurrentLapTime = telemetry.LapCurrentLapTime,
            LastLapTime = Math.Max(telemetry.LapLastLapTime, 0),
            LastLapTimeDelta = telemetry.LapLastLapTime <= 0 ? 0 : telemetry.LapDeltaToSessionLastlLap,
            BestLapTime = Math.Max(telemetry.LapBestLapTime, 0),
            BestLapTimeDelta = telemetry.LapDeltaToBestLap <= 0 ? 0 : telemetry.LapDeltaToBestLap,
            Gear = telemetry.Gear == -1 ? "R" : telemetry.Gear == 0 ? "N" : telemetry.Gear.ToString(),
            Rpm = (int)telemetry.RPM,
            RpmLights = [.. GenerateRpmLights(CarScreenName)],
            RpmMax = GetRpmMax(CarScreenName),
            Speed = (int)Math.Round(telemetry.Speed * 3.6),
            BrakePct = (int)Math.Round(telemetry.Brake * 100, 0),
            ThrottlePct = (int)Math.Round(telemetry.Throttle * 100, 0),
            BrakeBias = telemetry.DcBrakeBias,
            FuelRemaining = telemetry.FuelLevel,
            FuelLastLap = _fuelStintHistory.Count > 0 ? _fuelStintHistory[^1] : 0,
            FuelAvgLap = _fuelStintHistory.Count > 0 ? _fuelStintHistory.Average() : 0,
            // Don't calculate estimated laps until we have fuel usage history
            FuelEstLaps = _fuelStintHistory.Count > 0 && _fuelStintHistory.Average() > 0 
                ? (telemetry.FuelLevel / _fuelStintHistory.Average()) 
                : 0,
            AirTemp = telemetry.AirTemp,
            TrackTemp = telemetry.TrackTemp,
            PitLimiterOn = telemetry.EngineWarnings.HasFlag(EngineWarnings.PitSpeedLimiter),
            CarNumber = playerInfo?.CarNumber ?? string.Empty,
            // Convert steering angle to percentage (0-100)
            SteeringPct = (int)Math.Round(((telemetry.SteeringWheelAngle / telemetry.SteeringWheelAngleMax) * 100) + 50)
        };
        
        Console.WriteLine($"SteeringWheelAngle: {telemetry.SteeringWheelAngle}, SteeringWheelAngleMax: {telemetry.SteeringWheelAngleMax}, SteeringPct: {displayUpdate.SteeringPct}");

        return new DisplayUpdate { Type = DisplayType.RaceDashboard, Data = displayUpdate };
    }

    public static RpmLight[] GenerateRpmLights(string carName)
    {
        return carName switch
        {
            "FIA F4" => [
                new RpmLight { Rpm = 6300, Color = "Green" },
                new RpmLight { Rpm = 6500, Color = "Green" },
                new RpmLight { Rpm = 6600, Color = "Green" },
                new RpmLight { Rpm = 6700, Color = "Green" },
                new RpmLight { Rpm = 6800, Color = "Red" },
                new RpmLight { Rpm = 6900, Color = "Red" }
            ],
            _ => []
        };
    }

    public static int GetRpmMax(string carName)
    {
        return carName switch
        {
            "FIA F4" => 7000,
            _ => 0, // Default value for other cars
        };
    }
}
