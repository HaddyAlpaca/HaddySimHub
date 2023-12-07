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

            return new RaceData
            {
                SessionType = session.SessionType,
                DeltaTime = typedRawData.Telemetry.LapDeltaToSessionBestLap,
                IsTimedSession = session.IsLimitedTime,
                CurrentLap = typedRawData.Telemetry.Lap,
                //MaxIncidents = typedRawData.Telemetry.,
                //TotalLaps = 
                SessionTimeRemaining = (float)typedRawData.Telemetry.SessionTimeRemain,
                Position = typedRawData.Telemetry.PlayerCarPosition,
                Incidents = typedRawData.Telemetry.PlayerCarDriverIncidentCount,
                LastLapTime = typedRawData.Telemetry.LapLastLapTime,
                BestLapTime = typedRawData.Telemetry.LapDeltaToSessionBestLap,
                Gear = typedRawData.Telemetry.Gear,
                Rpm = (int)typedRawData.Telemetry.RPM,
                Speed = (int)Math.Round(typedRawData.Telemetry.Speed * 3.6),
                BrakeBias = typedRawData.Telemetry.dcBrakeBias,
                FuelRemaining = typedRawData.Telemetry.FuelLevel,
                AirTemp = typedRawData.Telemetry.AirTemp,
                TrackTemp = typedRawData.Telemetry.TrackTemp,
                ClutchPct = (int)typedRawData.Telemetry.Clutch * 100,
                ThrottlePct = (int)typedRawData.Telemetry.Throttle * 100,
                BrakePct = (int)typedRawData.Telemetry.Brake * 100
            };
        }
    }
}
