using HaddySimHub.GameData;
using HaddySimHub.GameData.Models;
using HaddySimHub.Logging;
using iRacingSDK;
using static iRacingSDK.SessionData._SessionInfo;

namespace HaddySimHub.IRacing;

public class GameDataReader : GameDataReaderBase
{
    private _Sessions? session;
    private SessionData? sessionData;
    private Sector? lastCompletedSector;
    private Sector? currentSector;

    public GameDataReader(ILogger logger)
        : base(logger)
    {
        iRacingSDK.iRacing.NewData += (DataSample obj) =>
        {
            this.UpdateRawData(obj);
        };
        iRacingSDK.iRacing.StartListening();
    }

    public override object Convert(object rawData)
    {
        if (rawData is not DataSample typedRawData)
        {
            throw new InvalidDataException("Received data is not of type DataSample");
        }

        var telemetry = typedRawData.Telemetry;

        if (this.session == null || this.session.SessionNum != telemetry.SessionNum)
        {
            // An new session has started, get session related information
            this.sessionData = typedRawData.SessionData;
            this.session = this.sessionData.SessionInfo.Sessions.First(s => s.SessionNum == telemetry.SessionNum);
        }

        if (this.session == null || this.sessionData == null)
        {
            throw new NullReferenceException("No session data available");
        }

        Car? carBehind = telemetry.RaceCars.FirstOrDefault(c => c.Position == telemetry.PlayerCarPosition + 1);
        Car? carAhead = telemetry.RaceCars.FirstOrDefault(c => c.Position == telemetry.PlayerCarPosition - 1);

        this.UpdateLastSectorTime(telemetry);

        return new RaceData
        {
            SessionType = this.session.SessionType,
            IsLimitedTime = this.session.IsLimitedTime,
            IsLimitedSessionLaps = this.session.IsLimitedSessionLaps,
            CurrentLap = telemetry.Lap,
            TotalLaps = this.session._SessionLaps,
            Incidents = telemetry.PlayerCarDriverIncidentCount,
            MaxIncidents = Math.Max(this.sessionData.WeekendInfo.WeekendOptions._IncidentLimit, 999),
            SessionTimeRemaining = (float)telemetry.SessionTimeRemain,
            Position = telemetry.PlayerCarPosition,
            StrengthOfField = telemetry.RaceCars.Count() > 1 ? (int)Math.Round(telemetry.RaceCars.Average(r => r.Details.Driver.IRating)) : 0,
            CurrentLapTime = telemetry.LapCurrentLapTime,
            LastSectorNum = this.lastCompletedSector?.SectorNum ?? 0,
            LastSectorTime = this.lastCompletedSector?.SectorTime ?? 0,
            LastLapTime = telemetry.LapLastLapTime,
            LastLapTimeDelta = telemetry.LapLastLapTime == 0 ? 0 : telemetry.LapDeltaToSessionLastlLap,
            BestLapTime = telemetry.LapBestLapTime,
            BestLapTimeDelta = telemetry.LapBestLapTime == 0 ? 0 : telemetry.LapDeltaToSessionBestLap,
            Gear = telemetry.Gear,
            Rpm = (int)telemetry.RPM,
            Speed = (int)Math.Round(telemetry.Speed * 3.6),
            BrakeBias = telemetry.dcBrakeBias,
            FuelRemaining = telemetry.FuelLevel,
            AirTemp = telemetry.AirTemp,
            TrackTemp = telemetry.TrackTemp,
            ClutchPct = (int)(telemetry.Clutch * 100),
            ThrottlePct = (int)(telemetry.Throttle * 100),
            BrakePct = (int)(telemetry.Brake * 100),
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
        };
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

    private void UpdateLastSectorTime(Telemetry telemetry)
    {
        // Get the sector the car is in
        var currentSector = this.sessionData!.SplitTimeInfo.Sectors
            .OrderBy(s => s.SectorStartPct)
            .FirstOrDefault(s => s.SectorStartPct >= telemetry.LapDistPct);

        if (currentSector != null)
        {
            this.logger.Debug($"Current sector: {currentSector}");

            if (this.currentSector == null)
            {
                // First sector of the session
                this.logger.Debug("First sector of the session");

                this.currentSector = new Sector
                {
                    LapNum = telemetry.Lap,
                    SectorNum = (int)currentSector.SectorNum,
                    SectorStartTime = telemetry.LapCurrentLapTime,
                };
            }
            else
            {
                this.logger.Debug($"this.currentSector: LapNum {this.currentSector.LapNum}, SectorNum: {this.currentSector.SectorNum}");
                this.logger.Debug($"telemetry.Lap: {telemetry.Lap}, currentSector.SectorNum: {currentSector.SectorNum}");
                if (this.currentSector.LapNum != telemetry.Lap ||
                    this.currentSector.SectorNum != currentSector.SectorNum)
                {
                    // The car has entered a new sector:
                    // - Set the end time of the previous sector
                    // - Set the start time of the new sector
                    if (currentSector.SectorNum == this.currentSector.SectorNum + 1 ||
                        (currentSector.SectorNum == 1 && telemetry.Lap == this.currentSector.LapNum + 1))
                    {
                        this.lastCompletedSector = new Sector
                        {
                            LapNum = this.currentSector.LapNum,
                            SectorNum = this.currentSector.SectorNum,
                            SectorStartTime = this.currentSector.SectorStartTime,
                            SectorEndTime = telemetry.LapCurrentLapTime,
                        };
                    }
                    else
                    {
                        this.lastCompletedSector = null;
                    }

                    this.currentSector = new Sector
                    {
                        LapNum = telemetry.Lap,
                        SectorNum = (int)currentSector.SectorNum,
                        SectorStartTime = telemetry.LapCurrentLapTime,
                    };
                }
            }
        }
    }
}
