using HaddySimHub.Models;

namespace HaddySimHub.Runners;

internal class TruckTestRunner : IRunner
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        bool parkingBrakeOn = false;
        while (!cancellationToken.IsCancellationRequested)
        {
            parkingBrakeOn = !parkingBrakeOn;
            var update = new DisplayUpdate
            {
                Type = DisplayType.TruckDashboard,
                Data = new TruckData
                {
                    Speed = (short)DateTime.Now.Second,
                    ParkingBrakeOn = parkingBrakeOn,
                }
            };
            await GameDataHub.SendDisplayUpdate(update);
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
        }
    }
}