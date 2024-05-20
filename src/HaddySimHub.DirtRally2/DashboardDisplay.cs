using HaddySimHub.GameData;
using HaddySimHub.GameData.Models;

namespace HaddySimHub.DirtRally2;

public sealed class DashboardDisplay : IDisplay
{
    public DisplayUpdate GetDisplayUpdate(object inputData)
    {
        var typedData = (Packet)inputData;

        var data = new RallyData
        {
            Speed = Convert.ToInt32(typedData.speed_ms * 3.6),
            Rpm = Convert.ToInt32(typedData.rpm * 10),
            MaxRpm = Convert.ToInt32(typedData.max_rpm),
            Gear = Convert.ToInt32(typedData.gear),
            CompletedPct = Math.Min(Convert.ToInt32(typedData.progress * 100), 100),
            DistanceTravelled = Math.Max(Convert.ToInt32(typedData.distance), 0),
            Position = Convert.ToInt32(typedData.car_pos),
            Sector1Time = typedData.sector_1_time,
            Sector2Time = typedData.sector_2_time,
            LapTime = typedData.lap_time,
        };

        return new DisplayUpdate { Type = DisplayType.RallyDashboard, Data = data };
    }
}