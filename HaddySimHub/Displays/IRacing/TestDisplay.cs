using HaddySimHub.Models;

namespace HaddySimHub.Displays.IRacing
{
    internal class TestDisplay(string name) : TestDisplayBase(name)
    {
        protected override DisplayUpdate GenerateDisplayUpdate()
        {
            // Simulate throttle and brake input
            // The brake and throttle should be opsite of each other and be a sinusoid wave
            double time = DateTime.Now.TimeOfDay.TotalSeconds;
            int brakePct = (int)((Math.Sin(time) + 1) * 50);
            int throttlePct = 100 - brakePct;

            return new DisplayUpdate
            {
                Type = DisplayType.RaceDashboard,
                Data = new RaceData
                {
                    Speed = (short)DateTime.Now.Second,
                    Gear = new Random().Next(1, 6).ToString(),
                    Rpm = (short)new Random().Next(0, 10000),
                    RpmLights = [.. Display.GenerateRpmLights("FIA F4")],
                    RpmMax = 7000,
                    TrackTemp = new Random().Next(10, 50),
                    AirTemp = new Random().Next(10, 50),
                    SessionType = "Practice",
                    IsLimitedTime = false,
                    BestLapTime = new Random().Next(60, 120),
                    BestLapTimeDelta = new Random().Next(-10, 10),
                    LastLapTime = new Random().Next(60, 120),
                    LastLapTimeDelta = new Random().Next(-10, 10),
                    BrakeBias = new Random().Next(0, 100),
                    CurrentLapTime = new Random().Next(60, 120),
                    PitLimiterOn = new Random().Next(0, 2) == 1,
                    CurrentLap = new Random().Next(1, 10),
                    LastSectorNum = new Random().Next(1, 3),
                    LastSectorTime = new Random().Next(10, 30),
                    FuelRemaining = new Random().Next(0, 100),
                    Incidents = new Random().Next(0, 10),
                    MaxIncidents = 17,
                    Position = new Random().Next(1, 20),
                    TotalLaps = new Random().Next(10, 20),
                    BrakePct = brakePct,
                    ThrottlePct = throttlePct,
                    CarNumber = "80",
                }
            };
        }
    }
}
