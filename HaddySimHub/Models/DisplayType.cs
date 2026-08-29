namespace HaddySimHub.Models;

// The values are sent over SSE as plain numbers (no JsonStringEnumConverter), and
// ClientApp/src/app/sse.service.ts mirrors them positionally. Append new members;
// inserting one shifts every dashboard after it.
public enum DisplayType
{
    None,
    TruckDashboard,
    RaceDashboard,
    RallyDashboard,
    FlightDashboard,
}
