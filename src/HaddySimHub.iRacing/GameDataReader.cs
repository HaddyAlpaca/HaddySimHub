using HaddySimHub.GameData;
using HaddySimHub.GameData.Models;
using HaddySimHub.Logging;
using iRacingSDK;

namespace HaddySimHub.IRacing
{
    public class GameDataReader : GameDataReaderBase
    {
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
            var session = typedRawData.SessionData.SessionInfo.Sessions.First(s => s.SessionNum == telemetry.SessionNum);

            // Set flag
            string flag = string.Empty;
            switch (telemetry.SessionFlags)
            {
                case SessionFlags.checkered:
                    break;
                case SessionFlags.white:
                    flag = "white";
                    break;
                case SessionFlags.green:
                    flag = "green";
                    break;
                case SessionFlags.yellow:
                    flag = "yellow";
                    break;
                case SessionFlags.red:
                    flag = "red";
                    break;
                case SessionFlags.blue:
                    flag = "blue";
                    break;
                case SessionFlags.yellowWaving:
                    flag = "yellow";
                    break;
                case SessionFlags.greenHeld:
                    flag = "green";
                    break;
                case SessionFlags.black:
                    flag = "black";
                    break;
                case SessionFlags.repair:
                    flag = "black-orange";
                    break;
            }

            Car? carBehind = telemetry.RaceCars.FirstOrDefault(c => c.Position == telemetry.PlayerCarClassPosition + 1);
            Car? carAhead = telemetry.RaceCars.FirstOrDefault(c => c.Position == telemetry.PlayerCarClassPosition - 1);

            long incidentLimit = typedRawData.SessionData.WeekendInfo.WeekendOptions._IncidentLimit;
            if (incidentLimit == long.MaxValue)
            {
                incidentLimit = 0;
            }

            return new RaceData
            {
                SessionType = session.SessionType,
                IsLimitedTime = session.IsLimitedTime,
                IsLimitedSessionLaps = session.IsLimitedSessionLaps,
                CurrentLap = telemetry.Lap,
                TotalLaps = session._SessionLaps,
                Incidents = telemetry.PlayerCarDriverIncidentCount,
                MaxIncidents = incidentLimit,
                SessionTimeRemaining = (float)telemetry.SessionTimeRemain,
                Position = telemetry.PlayerCarPosition,
                CurrentLapTime = telemetry.LapCurrentLapTime,
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
                Flag = flag,
                PitLimiterOn = telemetry.EngineWarnings.HasFlag(EngineWarnings.PitSpeedLimiter),
                DriverAheadName = carAhead?.Details.UserName ?? string.Empty,
                DriverAheadLicenseColor = carAhead?.Details.Driver.LicColor ?? string.Empty,
                DriverAheadCarNumber = carAhead?.Details.CarNumberDisplay ?? string.Empty,
                DriverAheadIRating = carAhead?.Details.Driver.IRating ?? 0,
                DriverAheadLicense = carAhead?.Details.Driver.LicString ?? string.Empty,
                DriverBehindName = carBehind?.Details.UserName ?? string.Empty,
            };
        }
    }
}
