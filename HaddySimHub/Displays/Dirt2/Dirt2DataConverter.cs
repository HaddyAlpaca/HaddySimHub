using HaddySimHub.Interfaces;
using HaddySimHub.Models;

namespace HaddySimHub.Displays.Dirt2;

public class Dirt2DataConverter : IDataConverter<Packet, DisplayUpdate>
{
    public DisplayUpdate Convert(Packet data)
    {
        var rpmMax = System.Convert.ToInt32(data.max_rpm * 10);

        var displayData = new RallyData
        {
            Speed = System.Convert.ToInt32(data.speed_ms * 3.6),
            Rpm = System.Convert.ToInt32(data.rpm * 10),
            RpmMax = rpmMax,
            Gear = data.gear == 0 ? "N" : data.gear < 0 ? "R" : data.gear.ToString(),
            Clutch = System.Convert.ToInt32(data.clutch * 100),
            Brake = System.Convert.ToInt32(data.brakes * 100),
            Throttle = System.Convert.ToInt32(data.throttle * 100),
            CompletedPct = Math.Min(System.Convert.ToInt32(data.progress * 100), 100),
            DistanceTravelled = Math.Max(System.Convert.ToInt32(data.distance), 0),
            Position = System.Convert.ToInt32(data.car_pos),
            Sector1Time = data.sector_1_time,
            Sector2Time = data.sector_2_time,
            LapTime = data.lap_time,
        };

        return new DisplayUpdate { Type = DisplayType.RallyDashboard, Data = displayData };
    }
}
