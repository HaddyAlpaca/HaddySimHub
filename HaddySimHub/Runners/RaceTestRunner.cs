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
                    DriverBehindName = "Driver Behind",
                    DriverBehindIRating = 1234,
                    DriverBehindCarNumber = "23",
                    DriverBehindLicense = "C",
                    DriverBehindLicenseColor = "#FF0000",
                    DriverBehindDelta = new Random().Next(1, 10),
                    DriverAheadName = "Driver Ahead",
                    DriverAheadIRating = 5678,
                    DriverAheadCarNumber = "45",
                    DriverAheadLicense = "A",
                    DriverAheadLicenseColor = "#00FF00",
                    DriverAheadDelta = new Random().Next(-10, -1),
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
                    TrackPositions = GenerateRandomTrackPositions(),
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
    private static TrackPosition[] GenerateRandomTrackPositions()
    {
        var trackPositions = new List<TrackPosition>();

        for (int i = 0; i < 5; i++)
        {
            trackPositions.Add(new TrackPosition
            {
                LapDistPct = (float)new Random().NextDouble() * 100,
                Status = TrackPositionStatus.SameLap,
            });
        }


        // Add player
        trackPositions.Add(new TrackPosition
        {
            LapDistPct = (float)new Random().NextDouble() * 100,
            Status = TrackPositionStatus.IsPlayer,
        });

        // Add a driver that is a lap ahead
        trackPositions.Add(new TrackPosition
        {
            LapDistPct = (float)new Random().NextDouble() * 100,
            Status = TrackPositionStatus.LapAhead,
        });

        // Add a pace car
        trackPositions.Add(new TrackPosition
        {
            LapDistPct = (float)new Random().NextDouble() * 100,
            Status = TrackPositionStatus.IsPaceCar,
        });

        // Add a car that is in the pits
        trackPositions.Add(new TrackPosition
        {
            LapDistPct = 0,
            Status = TrackPositionStatus.InPits,
        });

        return [.. trackPositions];
    }
}