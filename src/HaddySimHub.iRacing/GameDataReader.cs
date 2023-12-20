using HaddySimHub.GameData;
using HaddySimHub.GameData.Models;
using HaddySimHub.Logging;
using iRacingSDK;

namespace HaddySimHub.iRacing
{
    public class GameDataReader : GameDataReaderBase
    {
        public GameDataReader(ILogger logger): base(logger)
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

            var session = typedRawData.SessionData.SessionInfo.Sessions.First(s => s.SessionNum == typedRawData.Telemetry.SessionNum);
            
            //Set flag
            string flag = string.Empty;
            switch (typedRawData.Telemetry.SessionFlags)
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

            Car? carBehind = typedRawData.Telemetry.RaceCars.FirstOrDefault(c => c.Position == typedRawData.Telemetry.PlayerCarClassPosition + 1);
            Car? carAhead = typedRawData.Telemetry.RaceCars.FirstOrDefault(c => c.Position == typedRawData.Telemetry.PlayerCarClassPosition - 1);

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
                CurrentLap = typedRawData.Telemetry.Lap,
                TotalLaps = session._SessionLaps,
                Incidents = typedRawData.Telemetry.PlayerCarDriverIncidentCount,
                MaxIncidents = incidentLimit,
                SessionTimeRemaining = (float)typedRawData.Telemetry.SessionTimeRemain,
                Position = typedRawData.Telemetry.PlayerCarPosition,
                CurrentLapTime = typedRawData.Telemetry.LapCurrentLapTime,
                LastLapTime = typedRawData.Telemetry.LapLastLapTime,
                LastLapTimeDelta = typedRawData.Telemetry.LapDeltaToSessionLastlLap,
                BestLapTime = typedRawData.Telemetry.LapBestLapTime,
                BestLapTimeDelta = typedRawData.Telemetry.LapDeltaToSessionBestLap,
                Gear = typedRawData.Telemetry.Gear,
                Rpm = (int)typedRawData.Telemetry.RPM,
                Speed = (int)Math.Round(typedRawData.Telemetry.Speed * 3.6),
                BrakeBias = typedRawData.Telemetry.dcBrakeBias,
                FuelRemaining = typedRawData.Telemetry.FuelLevel,
                AirTemp = typedRawData.Telemetry.AirTemp,
                TrackTemp = typedRawData.Telemetry.TrackTemp,
                ClutchPct = (int)(typedRawData.Telemetry.Clutch * 100),
                ThrottlePct = (int)(typedRawData.Telemetry.Throttle * 100),
                BrakePct = (int)(typedRawData.Telemetry.Brake * 100),
                Flag = flag,
                PitLimiterOn = typedRawData.Telemetry.EngineWarnings.HasFlag(EngineWarnings.PitSpeedLimiter),
                DriverAheadName = carAhead?.Details.UserName ?? string.Empty,
                DriverAheadLicenseColor = carAhead?.Details.Driver.LicColor ?? string.Empty,
                DriverAheadCarNumber = carAhead?.Details.CarNumberDisplay ?? string.Empty,
                DriverAheadIRating = carAhead?.Details.Driver.IRating ?? 0,
                DriverAheadLicense = carAhead?.Details.Driver.LicString ?? string.Empty,
                DriverBehindName = carBehind?.Details.UserName ?? string.Empty
            };
        }
    }
}
