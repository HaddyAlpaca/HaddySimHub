using HaddySimHub.GameData;
using HaddySimHub.GameData.Models;
using iRacingSDK;

namespace HaddySimHub.iRacing
{
    public class GameDataReader : IGameDataReader
    {
        public event EventHandler<object>? RawDataUpdate;

        public GameDataReader()
        {
            iRacingSDK.iRacing.NewData += (DataSample obj) =>
            {
                this.RawDataUpdate?.Invoke(this, obj);
            };
            iRacingSDK.iRacing.StartListening();
        }
        public object Convert(object rawData)
        {
            if (rawData is not DataSample typedRawData)
            {
                return new RaceData();
            }

            var session = typedRawData.SessionData.SessionInfo.Sessions.First(s => s.SessionNum == typedRawData.Telemetry.SessionNum);
            var x = typedRawData.SessionData.DriverInfo.CompetingDrivers.First();
            
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
                DeltaTime = typedRawData.Telemetry.LapDeltaToSessionBestLap,
                IsTimedSession = session.IsLimitedTime,
                CurrentLap = typedRawData.Telemetry.Lap,
                TotalLaps = session._SessionLaps,
                Incidents = typedRawData.Telemetry.PlayerCarDriverIncidentCount,
                MaxIncidents = incidentLimit,
                SessionTimeRemaining = (float)typedRawData.Telemetry.SessionTimeRemain,
                Position = typedRawData.Telemetry.PlayerCarPosition,
                LastLapTime = typedRawData.Telemetry.LapLastLapTime,
                BestLapTime = typedRawData.Telemetry.LapDeltaToSessionBestLap,
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
                DriverAhead = carAhead?.Details.UserName ?? string.Empty,
                DriverBehind = carBehind?.Details.UserName ?? string.Empty
            };
        }
    }
}
