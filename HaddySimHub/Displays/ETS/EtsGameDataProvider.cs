using HaddySimHub.Interfaces;
using SCSSdkClient;
using SCSSdkClient.Object;

namespace HaddySimHub.Displays.ETS;

public class EtsGameDataProvider : IGameDataProvider<SCSTelemetry>
{
    private SCSSdkTelemetry? _telemetry;
    private readonly ISCSTelemetryFactory _telemetryFactory;

    public event EventHandler<SCSTelemetry>? DataReceived;

    public EtsGameDataProvider(ISCSTelemetryFactory telemetryFactory)
    {
        _telemetryFactory = telemetryFactory ?? throw new ArgumentNullException(nameof(telemetryFactory));
    }

    public void Start()
    {
        _telemetry = _telemetryFactory.Create();
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
