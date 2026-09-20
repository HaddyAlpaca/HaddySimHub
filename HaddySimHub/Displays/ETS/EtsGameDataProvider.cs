using HaddySimHub.Interfaces;
using SCSSdkClient;
using SCSSdkClient.Object;

namespace HaddySimHub.Displays.ETS;

public class EtsGameDataProvider : IGameDataProvider<SCSTelemetry>
{
    private SCSSdkTelemetry? _telemetry;

    public event EventHandler<SCSTelemetry>? DataReceived;

    public void Start()
    {
        _telemetry = new SCSSdkTelemetry();
        _telemetry.Data += HandleTelemetryData;
    }

    public void Stop()
    {
        if (_telemetry is null)
        {
            return;
        }

        _telemetry.Data -= HandleTelemetryData;
        _telemetry.Dispose();
        _telemetry = null;
    }

    private void HandleTelemetryData(SCSTelemetry data, bool newTimestamp)
    {
        DataReceived?.Invoke(this, data);
    }
}
