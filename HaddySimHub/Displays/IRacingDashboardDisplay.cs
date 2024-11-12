using HaddySimHub.Logging;
using HaddySimHub.Models;
using iRacingSDK;

namespace HaddySimHub.Displays;

internal sealed class IRacingDashboardDisplay(Func<DisplayUpdate, Task> updateDisplay, ILogger logger) : DisplayBase<DataSample>(updateDisplay)
{
    public override void Start()
    {
        iRacing.NewData += (data) => 
        {
            try
            {
                this._updateDisplay(this.ConvertToDisplayUpdate(data));
            }
            catch (Exception ex)
            {
                logger.Error($"{ex.Message}\n\n{ex.StackTrace}");
            }
        };
        iRacing.StartListening();
    }

    public override void Stop()
    {
        if (iRacing.IsConnected) {
            iRacing.StopListening();
        }
    }

    public override string Description => "IRacing";
    public override bool IsActive => Functions.IsProcessRunning("iracingui") && iRacingSDK.iRacing.IsConnected;

    protected override DisplayUpdate ConvertToDisplayUpdate(DataSample data)
    {
        var telemetry = data.Telemetry;

        var sessionData = data.SessionData;
        var session = sessionData.SessionInfo.Sessions.First(s => s.SessionNum == telemetry.SessionNum);

        Car? carBehind = null;
        Car? carAhead = null;
        if (session.IsRace)
        {
            carBehind = telemetry.RaceCars.FirstOrDefault(c => c.Position == telemetry.PlayerCarPosition + 1);
            carAhead = telemetry.RaceCars.FirstOrDefault(c => c.Position == telemetry.PlayerCarPosition - 1);
        }

        // Update track positions
        var playerCar = telemetry.Cars.First(c => c.CarIdx == telemetry.PlayerCarIdx);
        var trackPositions = telemetry.Cars
            .Where(c => !c.HasRetired && (!c.Details.IsPaceCar || c.Details.IsOnPitRoad))
            .Select(c => new TrackPosition
            {
                LapDistPct = c.DistancePercentage,
                Status =
                    c.CarIdx == telemetry.PlayerCarIdx ? TrackPositionStatus.IsPlayer :
                    c.Details.IsPaceCar ? TrackPositionStatus.IsPaceCar :
                    c.Details.IsOnPitRoad ? TrackPositionStatus.InPits :
                    telemetry.Session.IsRace && playerCar.TotalDistance - c.TotalDistance > .8 ? TrackPositionStatus.LapBehind :
                    telemetry.Session.IsRace && c.TotalDistance - playerCar.TotalDistance > .8 ? TrackPositionStatus.LapAhead :
                    TrackPositionStatus.SameLap,
            }).ToArray();

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
            BestLapTimeDelta = telemetry.LapBestLapTime <= 0 ? 0 : telemetry.LapDeltaToSessionBestLap,
            Gear = telemetry.Gear,
            Rpm = (int)telemetry.RPM,
            Speed = (int)Math.Round(telemetry.Speed * 3.6),
            BrakeBias = telemetry.dcBrakeBias,
            FuelRemaining = telemetry.FuelLevel,
            AirTemp = telemetry.AirTemp,
            TrackTemp = telemetry.TrackTemp,
            Flag = GetFlag(telemetry.SessionFlags),
            PitLimiterOn = telemetry.EngineWarnings.HasFlag(EngineWarnings.PitSpeedLimiter),
            DriverAheadName = carAhead?.Details.UserName ?? string.Empty,
            DriverAheadLicenseColor = carAhead?.Details.Driver.LicColor.Replace("0x", "#") ?? string.Empty,
            DriverAheadCarNumber = carAhead?.Details.CarNumberDisplay ?? string.Empty,
            DriverAheadIRating = carAhead?.Details.Driver.IRating ?? 0,
            DriverAheadLicense = carAhead?.Details.Driver.LicString ?? string.Empty,
            DriverBehindName = carBehind?.Details.UserName ?? string.Empty,
            DriverBehindLicenseColor = carBehind?.Details.Driver.LicColor.Replace("0x", "#") ?? string.Empty,
            DriverBehindCarNumber = carBehind?.Details.CarNumberDisplay ?? string.Empty,
            DriverBehindIRating = carBehind?.Details.Driver.IRating ?? 0,
            DriverBehindLicense = carBehind?.Details.Driver.LicString ?? string.Empty,
            TrackPositions = trackPositions,
        };

        return new DisplayUpdate{ Type = DisplayType.RaceDashboard, Data = displayUpdate };
    }

    private static string GetFlag(SessionFlags sessionFlags)
    {
        return sessionFlags switch
        {
            SessionFlags.white => "white",
            SessionFlags.green => "green",
            SessionFlags.yellow => "yellow",
            SessionFlags.red => "red",
            SessionFlags.blue => "blue",
            SessionFlags.yellowWaving => "yellow",
            SessionFlags.greenHeld => "green",
            SessionFlags.black => "black",
            SessionFlags.repair => "black-orange",
            _ => string.Empty,
        };
    }
}