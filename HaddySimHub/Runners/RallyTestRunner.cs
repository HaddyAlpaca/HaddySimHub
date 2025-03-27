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
                }
            };
            await GameDataHub.SendDisplayUpdate(update);
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
        }
    }
}