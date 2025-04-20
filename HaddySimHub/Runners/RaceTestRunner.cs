using HaddySimHub.Models;

namespace HaddySimHub.Runners;

internal class RaceTestRunner : IRunner
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        int flag = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            // Simulate throttle and brake input
            // The brake and throttle should be opsite of each other and be a sinusoid wave
            double time = DateTime.Now.TimeOfDay.TotalSeconds;
            int brakePct = (int)((Math.Sin(time) + 1) * 50);
            int throttlePct = 100 - brakePct;

            var update = new DisplayUpdate
            {
                Type = DisplayType.RaceDashboard,
                Data = new RaceData
                {
                    Speed = (short)DateTime.Now.Second,
                    Gear = (short)new Random().Next(-1, 7),
                    Rpm = (short)new Random().Next(0, 10000),
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
                    TimingEntries = GenerateTimingEntries(),
                    BrakePct = brakePct,
                    ThrottlePct = throttlePct,
                    Flag = flag switch
                    {
                        1 => "yellow",
                        2 => "red",
                        3 => "black",
                        4 => "white",
                        5 => "blue",
                        6 => "red-yellow",
                        7 => "black-orange",
                        8 => "checkered",
                        _ => "green",
                    },
                }
            };
            await GameDataHub.SendDisplayUpdate(update);
            
            flag = flag == 8 ? 0 : flag + 1;

            await Task.Delay(TimeSpan.FromSeconds(.5), cancellationToken);
        }
    }
    private static TimingEntry[] GenerateTimingEntries()
    {
        var entries = new List<TimingEntry>();
        int lapsCompleted = 5;
        for (int i = 0; i < 5; i++)
        {
            entries.Add(new TimingEntry
            {
                DriverName = $"Driver {i + 1}",
                CarNumber = $"{i + 1}",
                License = $"A 1.{i}",
                LicenseColor = "#ff0000",
                IRating = 1000 + i * 500,
                Laps = lapsCompleted,
                LapCompletedPct = (float)new Random().NextDouble() * 100,
            });
        }


        // Add player
        entries.Add(new TimingEntry
        {
            DriverName = "Player",
            CarNumber = "80",
            License = "A 1.2k",
            LicenseColor = "#00ff00",
            IRating = 1200,
            Laps = lapsCompleted,
            LapCompletedPct = (float)new Random().NextDouble() * 100,
            IsPlayer = true,
        });

        // Add a driver that is a lap ahead
        entries.Add(new TimingEntry
        {
            DriverName = "Driver 6",
            CarNumber = "6",
            License = "D 1.3k",
            LicenseColor = "#0000ff",
            IRating = 1300,
            Laps = lapsCompleted + 1,
            LapCompletedPct = (float)new Random().NextDouble() * 100,
        });

        // Add a safety car
        entries.Add(new TimingEntry
        {
            DriverName = "Safety Car",
            CarNumber = "0",
            Laps = lapsCompleted,
            LapCompletedPct = (float)new Random().NextDouble() * 100,
            IsSafetyCar = true,
        });

        // Add a car that is in the pits
        entries.Add(new TimingEntry
        {
            DriverName = "Driver 7",
            CarNumber = "7",
            License = "C 1.4k",
            LicenseColor = "#ffff00",
            IRating = 1400,
            Laps = lapsCompleted,
            LapCompletedPct = 0,
            IsInPits = true,
        });

        return [.. entries];
    }
}