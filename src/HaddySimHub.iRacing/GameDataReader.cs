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

            return new RaceData
            {
                Gear = typedRawData.Telemetry.Gear,
                Rpm = (int)typedRawData.Telemetry.RPM,
                AirTemp = typedRawData.Telemetry.AirTemp,
                TrackTemp = typedRawData.Telemetry.TrackTemp,
                Laps = typedRawData.Telemetry.Lap,
                ThrottlePct = (int)typedRawData.Telemetry.Throttle * 100,
                BrakePct = (int)typedRawData.Telemetry.Brake * 100,
                DeltaTime = typedRawData.Telemetry.LapDeltaToSessionBestLap
            };
        }
    }
}
