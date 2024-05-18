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
            CompletedPct = Convert.ToInt32(typedData.progress * 100),
            DistanceTravelled = Convert.ToInt32(typedData.distance),
            Sector = Convert.ToInt32(typedData.sector),
            Sector1Time = typedData.sector_1_time,
            Sector2Time = typedData.sector_2_time,
            LapTime = typedData.lap_time,
            Position = Convert.ToInt32(typedData.car_pos),
            TimeElapsed = typedData.run_time,
        };

        return new DisplayUpdate { Type = DisplayType.RallyDashboard, Data = data };
    }
}