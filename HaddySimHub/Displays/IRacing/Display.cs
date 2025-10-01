using HaddySimHub.Models;
using HaddySimHub.Shared;
using iRacingSDK;

namespace HaddySimHub.Displays.IRacing;

internal sealed class Display() : DisplayBase<DataSample>()
{
    private int[]? _lastLaps;
    private int? _sessionNum;

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

        _lastLaps ??= (int[])telemetry.CarIdxLap.Clone();

        if (_sessionNum != telemetry.SessionNum)
        {
            _sessionNum = telemetry.SessionNum;
            _lastLaps = (int[])telemetry.CarIdxLap.Clone();
        }

        string CarScreenName = string.Empty;
        var playerInfo = sessionData.DriverInfo.CompetingDrivers.FirstOrDefault(d => d.CarIdx == telemetry.PlayerCarIdx);

        var qualifyingSession = sessionData.SessionInfo.Sessions.FirstOrDefault(s => s.SessionType.Contains("Qualify"));
        long startingPosition = 0;
        if (qualifyingSession?.ResultsPositions != null)
        {
            var playerQualifyingResult = qualifyingSession.ResultsPositions.FirstOrDefault(r => r.CarIdx == telemetry.PlayerCarIdx);
            if (playerQualifyingResult != null)
            {
                startingPosition = playerQualifyingResult.Position;
            }
        }

        var displayUpdate = new RaceData
        {
            SessionType = session.SessionType,
            IsLimitedTime = session.IsLimitedTime,
            IsLimitedSessionLaps = session.IsLimitedSessionLaps,
            CurrentLap = telemetry.Lap,
            TotalLaps = session._SessionLaps,
            Incidents = Math.Max(telemetry.PlayerCarDriverIncidentCount, 0),
            MaxIncidents = Math.Max(Math.Min(sessionData!.WeekendInfo.WeekendOptions._IncidentLimit, 999), 0),
            SessionTimeRemaining = (float)telemetry.SessionTimeRemain,
            Position = telemetry.PlayerCarPosition,
            StrengthOfField = telemetry.RaceCars.Count() > 1 ? (int)Math.Round(telemetry.RaceCars.Average(r => r.Details.Driver.IRating)) : 0,
            CurrentLapTime = telemetry.LapCurrentLapTime,
            LastLapTime = Math.Max(telemetry.LapLastLapTime, 0),
            LastLapTimeDelta = telemetry.LapLastLapTime <= 0 ? 0 : telemetry.LapDeltaToSessionLastlLap,
            BestLapTime = Math.Max(telemetry.LapBestLapTime, 0),
            BestLapTimeDelta = telemetry.LapDeltaToSessionBestLap <= 0 ? 0 : telemetry.LapDeltaToSessionBestLap,
            Gear = telemetry.Gear == -1 ? "R" : telemetry.Gear == 0 ? "N" : telemetry.Gear.ToString(),
            Rpm = (int)telemetry.RPM,
            RpmLights = [.. GenerateRpmLights(CarScreenName)],
            RpmMax = GetRpmMax(CarScreenName),
            Speed = (int)Math.Round(telemetry.Speed * 3.6),
            BrakePct = (int)Math.Round(telemetry.Brake * 100, 0),
            ThrottlePct = (int)Math.Round(telemetry.Throttle * 100, 0),
            BrakeBias = telemetry.DcBrakeBias,
            FuelRemaining = telemetry.FuelLevel,
            AirTemp = telemetry.AirTemp,
            TrackTemp = telemetry.TrackTemp,
            PitLimiterOn = telemetry.EngineWarnings.HasFlag(EngineWarnings.PitSpeedLimiter),
            CarNumber = playerInfo?.CarNumber ?? string.Empty,
        };

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
