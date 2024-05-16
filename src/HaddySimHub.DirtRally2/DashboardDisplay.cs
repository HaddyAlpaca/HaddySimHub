using F12020Telemetry;
using HaddySimHub.GameData;
using HaddySimHub.GameData.Models;

public sealed class DashboardDisplay : IDisplay
{
    public DisplayUpdate GetDisplayUpdate(object inputData)
    {
        var typedData = (CarTelemetryData)inputData;

        var data = new RaceData
        {
            Speed = typedData.speed,
            Rpm = typedData.engineRPM,
            Gear = typedData.gear,
        };

        return new DisplayUpdate { Type = DisplayType.RaceDashboard, Data = data };
    }
}