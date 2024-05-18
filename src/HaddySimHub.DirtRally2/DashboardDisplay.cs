using HaddySimHub.GameData;
using HaddySimHub.GameData.Models;

namespace HaddySimHub.DirtRally2;

public sealed class DashboardDisplay : IDisplay
{
    public DisplayUpdate GetDisplayUpdate(object inputData)
    {
        var typedData = (Packet)inputData;

        var data = new RaceData
        {
            Speed = Convert.ToInt32(typedData.speed_ms * 3.6),
            Rpm = Convert.ToInt32(typedData.rpm * 10),
            Gear = Convert.ToInt32(typedData.gear),
        };

        return new DisplayUpdate { Type = DisplayType.RaceDashboard, Data = data };
    }
}