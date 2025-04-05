using System.Text;
using HaddySimHub.Models;
using HaddySimHub.Shared;
using iRacingSDK;

namespace HaddySimHub.Displays;

internal sealed class IRacingDashboardDisplay(Func<DisplayUpdate, Task> updateDisplay) : DisplayBase<DataSample>(updateDisplay)
{
    private int[]? _lastLaps;
    private int? _sessionNum;

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
                Logger.Error($"{ex.Message}\n\n{ex.StackTrace}");
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
    public override bool IsActive => ProcessHelper.IsProcessRunning("iracingui");

    protected override DisplayUpdate ConvertToDisplayUpdate(DataSample data)
    {
        var telemetry = data.Telemetry;

        var sessionData = data.SessionData;
        var session = sessionData.SessionInfo.Sessions.First(s => s.SessionNum == telemetry.SessionNum);

        this._lastLaps ??= (int[])telemetry.CarIdxLap.Clone();

        if (this._sessionNum != telemetry.SessionNum)
        {
            this._sessionNum = telemetry.SessionNum;
            this._lastLaps = (int[])telemetry.CarIdxLap.Clone();
        }

        foreach (var driver in sessionData.DriverInfo.CompetingDrivers)
        {
            var carIdx = (int)driver.CarIdx;

            if (!telemetry.HasData(carIdx))
            {
                Logger.Error($"Car {carIdx} does not have telemetry data.");
                continue;
            }

            var carIdxLap = telemetry.CarIdxLap[carIdx];
            var carIdxLapDistPct = telemetry.CarIdxLapDistPct[carIdx];
            if (carIdxLap > this._lastLaps[carIdx] && carIdxLapDistPct > 0.80f)
            {
                // The car has passed the start/finish line and the percentage is in the 80% range.
                // Set the percentage to 0% to avoid the bug in iRacing data stream.
                carIdxLapDistPct = 0;
            }

            this._lastLaps[carIdx] = carIdxLap;

            // Build a log message
            var logMessage = new StringBuilder();
            logMessage.AppendLine($"*** Car {carIdx} telemetry data ***");
            logMessage.AppendLine($"Lap: {carIdxLap}");
            logMessage.AppendLine($"LapDistPct: {telemetry.CarIdxLapDistPct[carIdx]} (Corrected: {carIdxLapDistPct})");
            logMessage.AppendLine($"Distance: {telemetry.CarIdxDistance}");
            logMessage.AppendLine($"TotalDistance: {carIdxLap + carIdxLapDistPct}");
            logMessage.AppendLine($"Username: {driver.UserName}");
            logMessage.AppendLine($"CarNumber: {driver.CarNumber}");
            logMessage.AppendLine($"License color {driver.LicColor}");
            logMessage.AppendLine($"License string {driver.LicString}");
            logMessage.AppendLine($"License level {driver.LicLevel}");
            logMessage.AppendLine($"License sublevel {driver.LicSubLevel}");
            logMessage.AppendLine($"IRating: {driver.IRating}");
            logMessage.AppendLine($"In pit: {telemetry.CarIdxOnPitRoad}");
            logMessage.AppendLine($"Est time: {telemetry.CarIdxEstTime}");
            Logger.Debug(logMessage.ToString());
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

        Car? carBehind = null;
        Car? carAhead = null;
        if (session.IsRace)
        {
            carBehind = telemetry.RaceCars.FirstOrDefault(c => c.Position == telemetry.PlayerCarPosition + 1);
            carAhead = telemetry.RaceCars.FirstOrDefault(c => c.Position == telemetry.PlayerCarPosition - 1);
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
            BestLapTimeDelta = telemetry.LapBestLapTime <= 0 ? 0 : telemetry.LapDeltaToSessionBestLap,
            Gear = telemetry.Gear,
            Rpm = (int)telemetry.RPM,
            Speed = (int)Math.Round(telemetry.Speed * 3.6),
            BrakeBias = telemetry.DcBrakeBias,
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

        StringBuilder sb = new();
        sb.AppendLine("Track positions");
        foreach(var p in trackPositions)
        {
            Logger.Debug($"{p.LapDistPct}: {p.Status}");
        }

        return new DisplayUpdate{ Type = DisplayType.RaceDashboard, Data = displayUpdate };
    }

    private static string GetFlag(SessionFlags sessionFlags)
    {
        var flag = sessionFlags switch
        {
            SessionFlags.white => "white",
            SessionFlags.green or SessionFlags.greenHeld => "green",
            SessionFlags.yellow or SessionFlags.yellowWaving or SessionFlags.caution or SessionFlags.cautionWaving => "yellow",
            SessionFlags.red => "red",
            SessionFlags.blue => "blue",
            SessionFlags.black => "black",
            SessionFlags.repair => "black-orange",
            SessionFlags.debris => "red-yellow",
            SessionFlags.checkered => "checkered",
            _ => null,
        };

        if (flag is null)
        {
            Logger.Error($"Unknown flag: {sessionFlags}");
            flag = "green";
        }

        return flag;
    }
}
