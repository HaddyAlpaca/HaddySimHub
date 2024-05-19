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
            DistanceTravelled = Math.Max(Convert.ToInt32(typedData.distance), 0),
            Sector = Convert.ToInt32(typedData.sector) + 1,
            Sector1Time = typedData.sector_1_time,
            Sector2Time = typedData.sector_2_time,
            Position = Convert.ToInt32(typedData.car_pos),
            TimeElapsed = typedData.lap_time,
            TyrePressFl = typedData.tyre_pressure_fl,
            TyrePressFr = typedData.tyre_pressure_fr,
            TyrePressRl = typedData.tyre_pressure_rl,
            TyrePressRr = typedData.tyre_pressure_rr,
            BrakeTempFl = typedData.brakes_temp_fl,
            BrakeTempFr = typedData.brakes_temp_fr,
            BrakeTempRl = typedData.brakes_temp_rl,
            BrakeTempRr = typedData.brakes_temp_rr,
        };

        return new DisplayUpdate { Type = DisplayType.RallyDashboard, Data = data };
    }
}