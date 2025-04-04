using HaddySimHub.Models;

namespace HaddySimHub.Runners;

internal class RallyTestRunner : IRunner
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var update = new DisplayUpdate
            {
                Type = DisplayType.RallyDashboard,
                Data = new RallyData
                {
                    Speed = (short)DateTime.Now.Second,
                    CompletedPct = (short)DateTime.Now.Second,
                    DistanceTravelled = (short)DateTime.Now.Millisecond,
                    Gear = new Random().Next(-1, 6),
                    Rpm = new Random().Next(0, 10000),
                    LapTime = new Random().Next(0, 100),
                    Sector1Time = new Random().Next(0, 100),
                    Sector2Time = new Random().Next(0, 100),
                }
            };
            await GameDataHub.SendDisplayUpdate(update);
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
        }
    }
}