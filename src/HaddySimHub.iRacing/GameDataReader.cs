using HaddySimHub.GameData;
using HaddySimHub.GameData.Models;
using HaddySimHub.Logging;
using iRacingSDK;
using static iRacingSDK.SessionData._SessionInfo;

namespace HaddySimHub.IRacing
{
    public class GameDataReader : GameDataReaderBase
    {
        private _Sessions? session;
        private SessionData? sessionData;

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

            Car? carBehind = telemetry.RaceCars.FirstOrDefault(c => c.Position == telemetry.PlayerCarPosition + 1);
            Car? carAhead = telemetry.RaceCars.FirstOrDefault(c => c.Position == telemetry.PlayerCarPosition - 1);

            return new RaceData
            {
                SessionType = this.session.SessionType,
                IsLimitedTime = this.session.IsLimitedTime,
                IsLimitedSessionLaps = this.session.IsLimitedSessionLaps,
                CurrentLap = telemetry.Lap,
                TotalLaps = this.session._SessionLaps,
                Incidents = telemetry.PlayerCarDriverIncidentCount,
                MaxIncidents = this.sessionData.WeekendInfo.WeekendOptions._IncidentLimit == long.MaxValue ? 0 : this.sessionData.WeekendInfo.WeekendOptions._IncidentLimit,
                SessionTimeRemaining = (float)telemetry.SessionTimeRemain,
                Position = telemetry.PlayerCarPosition,
                StrengthOfField = telemetry.RaceCars.Count() > 1 ? (int)Math.Round(telemetry.RaceCars.Average(r => r.Details.Driver.IRating)) : 0,
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
    }
}
