using HaddySimHub.GameData;
using HaddySimHub.GameData.Models;
using HaddySimHub.Logging;
using iRacingSDK;
using static iRacingSDK.SessionData._SessionInfo;

namespace HaddySimHub.iRacing;

/// <summary>
/// Information about the current sector and the last completed sector of a car.
/// </summary>
internal class CarSectorInfo
{
    /// <summary>
    /// Gets or sets the current sector number.
    /// </summary>
    public int? CurrentSectorNum { get; set; }

    /// <summary>
    /// Gets the cars sectors.
    /// </summary>
    public List<Sector> Sectors { get; init; } = new ();

    /// <summary>
    /// Inserts or updates sector information.
    /// </summary>
    /// <param name="sector">Sector info update.</param>
    public void UpdateSector(Sector sector)
    {
        var existingSector = this.Sectors.FirstOrDefault(s => s.SectorNum == sector.SectorNum);
        if (existingSector == null)
        {
            this.Sectors.Add(sector);
        }
        else
        {
            existingSector = sector;
        }
    }

    /// <summary>
    /// Get sector info for given sector.
    /// </summary>
    /// <param name="sectorNum">Sector number.</param>
    /// <returns>Sector info for the given sector number or null when no info is available.</returns>
    public Sector? GetSector(int sectorNum) => this.Sectors.FirstOrDefault(s => s.SectorNum == sectorNum);

    // /// <summary>
    // /// Gets or sets the current sector info.
    // /// </summary>
    // public Sector? CurrentSector { get; set; }

    // /// <summary>
    // /// Gets or sets the last completed sector info.
    // /// </summary>
    // public Sector? LastCompletedSector { get; set; }
}

public class GameDataReader(ILogger logger) : GameDataReaderBase(logger)
{
    private readonly Dictionary<int, CarSectorInfo> carSectors = new ();
    private _Sessions? session;
    private SessionData? sessionData;

    public override void Initialize()
    {
        iRacingSDK.iRacing.NewData += this.UpdateRawData;
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

            // Reset car sector information
            this.carSectors.Clear();
        }

        if (this.session == null || this.sessionData == null)
        {
            throw new NullReferenceException("No session data available");
        }

        Car? carBehind = null;
        Car? carAhead = null;
        if (this.session.IsRace)
        {
            carBehind = telemetry.RaceCars.FirstOrDefault(c => c.Position == telemetry.PlayerCarPosition + 1);
            carAhead = telemetry.RaceCars.FirstOrDefault(c => c.Position == telemetry.PlayerCarPosition - 1);
        }

        // Update sector time information
        this.UpdateSectorTimes(telemetry);

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

        return new RaceData
        {
            SessionType = this.session.SessionType,
            IsLimitedTime = this.session.IsLimitedTime,
            IsLimitedSessionLaps = this.session.IsLimitedSessionLaps,
            CurrentLap = telemetry.Lap,
            TotalLaps = this.session._SessionLaps,
            Incidents = Math.Max(telemetry.PlayerCarDriverIncidentCount, 0),
            MaxIncidents = Math.Max(Math.Min(this.sessionData.WeekendInfo.WeekendOptions._IncidentLimit, 999), 0),
            SessionTimeRemaining = (float)telemetry.SessionTimeRemain,
            Position = telemetry.PlayerCarPosition,
            StrengthOfField = telemetry.RaceCars.Count() > 1 ? (int)Math.Round(telemetry.RaceCars.Average(r => r.Details.Driver.IRating)) : 0,
            CurrentLapTime = telemetry.LapCurrentLapTime,
            // LastSectorNum = this.carSectors[telemetry.PlayerCarIdx].LastCompletedSector?.SectorNum + 1 ?? 0,
            // LastSectorTime = this.carSectors[telemetry.PlayerCarIdx].LastCompletedSector?.SectorTime ?? 0,
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

    private void UpdateSectorTimes(Telemetry telemetry)
    {
        // foreach (var c in telemetry.RaceCars)
        // {
        //     // Get the sector the car is in
        //     var currentSector = this.sessionData!.SplitTimeInfo.Sectors
        //         .OrderBy(s => s.SectorStartPct)
        //         .FirstOrDefault(s => s.SectorStartPct >= c.DistancePercentage);

        //     if (currentSector != null)
        //     {
        //         // Get the current sector info of the car.
        //         if (!this.carSectors.TryGetValue(c.CarIdx, out CarSectorInfo? carSectorInfo))
        //         {
        //             carSectorInfo = new CarSectorInfo();
        //             this.carSectors.Add(c.CarIdx, carSectorInfo);
        //         }

        //         if (
        //             carSectorInfo.CurrentSectorNum != null &&
        //             currentSector.SectorNum != carSectorInfo.CurrentSectorNum)
        //         {
        //             // New sector entered.
        //             // - Update time stamp on completed sector.
        //             var sector = carSectorInfo.GetSector((int)carSectorInfo.CurrentSectorNum);
        //             sector = new Sector
        //             {
        //                 LapNum = sector.LapNum,
        //                 SectorNum = carSectorInfo.CurrentSector.SectorNum,
        //                 SectorStartTime = carSectorInfo.CurrentSector.SectorStartTime,
        //                 SectorEndTime = telemetry.SessionTime,
        //             };
        //         }

        //         if (carSectorInfo.CurrentSectorNum == null)
        //         {
        //             // First sector of the session
        //             this.logger.Debug($"Car {c.CarIdx}, First sector of the session");

        //             carSectorInfo.CurrentSectorNum = (int)currentSector.SectorNum;
        //             carSectorInfo.UpdateSector(new Sector
        //             {
        //                 LapNum = c.Lap,
        //                 SectorNum = (int)currentSector.SectorNum,
        //                 SectorStartTime = telemetry.SessionTime,
        //             });
        //         }
        //         else
        //         {
        //             this.logger.Debug($"Car {c.CarIdx}, this.currentSector: LapNum {carSectorInfo.CurrentSector.LapNum}, SectorNum: {carSectorInfo.CurrentSector.SectorNum}");
        //             this.logger.Debug($"Car {c.CarIdx}, telemetry.Lap: {c.Lap}, currentSector.SectorNum: {currentSector.SectorNum}");
        //             if (carSectorInfo.CurrentSector.LapNum != c.Lap ||
        //                 carSectorInfo.CurrentSector.SectorNum != currentSector.SectorNum)
        //             {
        //                 // The car has entered a new sector:
        //                 // - Set the end time of the previous sector
        //                 // - Set the start time of the new sector
        //                 if (currentSector.SectorNum == carSectorInfo.CurrentSector.SectorNum + 1 ||
        //                     (currentSector.SectorNum == 1 && telemetry.Lap == carSectorInfo.CurrentSector.LapNum + 1))
        //                 {
        //                     carSectorInfo.LastCompletedSector = new Sector
        //                     {
        //                         LapNum = carSectorInfo.CurrentSector.LapNum,
        //                         SectorNum = carSectorInfo.CurrentSector.SectorNum,
        //                         SectorStartTime = carSectorInfo.CurrentSector.SectorStartTime,
        //                         SectorEndTime = telemetry.SessionTime,
        //                     };
        //                 }
        //                 else
        //                 {
        //                     carSectorInfo.LastCompletedSector = null;
        //                 }

        //                 carSectorInfo.CurrentSector = new Sector
        //                 {
        //                     LapNum = c.Lap,
        //                     SectorNum = (int)currentSector.SectorNum,
        //                     SectorStartTime = telemetry.SessionTime,
        //                 };
        //             }
        //         }
        //     }
        // }
    }
}
