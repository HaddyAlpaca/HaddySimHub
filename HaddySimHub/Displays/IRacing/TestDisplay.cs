using HaddySimHub.Models;
using HaddySimHub.Interfaces;
using HaddySimHub.Services;

namespace HaddySimHub.Displays.IRacing
{
    public class TestDisplay : TestDisplayBase
    {
        public TestDisplay(
            string id,
            IDataConverter<DisplayUpdate, DisplayUpdate> identityDataConverter,
            IDisplayUpdateSender displayUpdateSender)
            : base(id, identityDataConverter, displayUpdateSender)
        {
        }

        protected override DisplayUpdate GenerateDisplayUpdate()
        {
            return new DisplayUpdate
            {
                Type = DisplayType.RaceDashboard,
                Data = new RaceData
                {
                    Speed = (short)DateTime.Now.Second,
                    Gear = _random.Next(1, 6).ToString(),
                    Rpm = (short)_random.Next(0, 10000),
                    RpmLights = [.. IRacingDataConverter.GenerateRpmLights("FIA F4")], // Use the static method from IRacingDataConverter
                    RpmMax = 7000,
                    TrackTemp = _random.Next(10, 50),
                    AirTemp = _random.Next(10, 50),
                    SessionType = "Practice",
                    IsLimitedTime = false,
                    BestLapTime = _random.Next(60, 120),
                    BestLapTimeDelta = _random.Next(-10, 10),
                    LastLapTime = _random.Next(60, 120),
                    LastLapTimeDelta = _random.Next(-10, 10),
                    BrakeBias = _random.Next(0, 100),
                    CurrentLapTime = _random.Next(60, 120),
                    PitLimiterOn = _random.Next(0, 2) == 1,
                    CurrentLap = _random.Next(1, 10),
                    LastSectorNum = _random.Next(1, 3),
                    LastSectorTime = _random.Next(10, 30),
                    FuelRemaining = _random.Next(0, 100),
                    Incidents = _random.Next(0, 10),
                    MaxIncidents = 17,
                    Position = new Random().Next(1, 20),
                    TotalLaps = new Random().Next(10, 20),
                    BrakePct = 20,
                    ThrottlePct = 80,
                    SteeringPct = 40,
                    CarNumber = "80",
                }
            };
        }
    }
}
