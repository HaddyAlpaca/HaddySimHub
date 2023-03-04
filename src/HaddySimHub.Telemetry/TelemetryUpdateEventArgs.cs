using HaddySimHub.Telemetry.Models;

namespace HaddySimHub.Telemetry
{
    public sealed class TelemetryUpdateEventArgs: EventArgs
    {
        public TelemetryType TelemetryType { get; set; } = TelemetryType.Unknown;
        public object Data { get; init; }

        public TelemetryUpdateEventArgs(object data)
        {
            if (data is RaceData)
                TelemetryType = TelemetryType.Race;
            else if (data is TruckData)
                TelemetryType = TelemetryType.Truck;

            Data = data;
        }
    }
}