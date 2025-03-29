using HaddySimHub.Models;

namespace HaddySimHub.Runners;

internal class RaceTestRunner : IRunner
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var update = new DisplayUpdate
            {
                Type = DisplayType.RaceDashboard,
                Data = new RaceData
                {
                    Speed = (short)DateTime.Now.Second,
                    Gear = (short)new Random().Next(-1, 7),
                    Rpm = (short)new Random().Next(0, 10000),
                    TrackTemp = (float)new Random().Next(10, 50),
                    AirTemp = (float)new Random().Next(10, 50),
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
                    BestLapTime = (float)new Random().Next(60, 120),
                    BestLapTimeDelta = new Random().Next(-10, 10),
                    LastLapTime = (float)new Random().Next(60, 120),
                    LastLapTimeDelta = new Random().Next(-10, 10),
                    BrakeBias = (float)new Random().Next(0, 100),
                    CurrentLapTime = (float)new Random().Next(60, 120),
                    PitLimiterOn = new Random().Next(0, 2) == 1,
                    CurrentLap = new Random().Next(1, 10),
                    LastSectorNum = new Random().Next(1, 3),
                    LastSectorTime = (float)new Random().Next(10, 30),
                    FuelRemaining = (float)new Random().Next(0, 100),
                    Incidents = new Random().Next(0, 10),
                    MaxIncidents = 17,
                    Position = new Random().Next(1, 20),
                    TotalLaps = new Random().Next(10, 20),
                    TrackPositions = GenerateRandomTrackPositions(),
                }
            };
            await GameDataHub.SendDisplayUpdate(update);
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
        }
    }
    private TrackPosition[] GenerateRandomTrackPositions()
    {
        var trackPositions = new List<TrackPosition>();

        for (int i = 0; i < 5; i++)
        {
            trackPositions.Add(new TrackPosition
            {
                LapDistPct = (float)new Random().NextDouble(),
                Status = TrackPositionStatus.SameLap,
            });
        }

        // Add a driver that is a lap ahead
        trackPositions.Add(new TrackPosition
        {
            LapDistPct = 0,
            Status = TrackPositionStatus.LapAhead,
        });

        // Add a pace car
        trackPositions.Add(new TrackPosition
        {
            LapDistPct = (float)new Random().NextDouble(),
            Status = TrackPositionStatus.IsPaceCar,
        });

        // Add a car that is in the pits
        trackPositions.Add(new TrackPosition
        {
            LapDistPct = (float)new Random().NextDouble(),
            Status = TrackPositionStatus.InPits,
        });

        return [.. trackPositions];
    }
}