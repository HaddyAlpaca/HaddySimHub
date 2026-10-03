using HaddySimHub.Capture;
using HaddySimHub.Displays;
using HaddySimHub.Interfaces;
using SCSSdkClient;
using SCSSdkClient.Object;

namespace HaddySimHub.Displays.ETS;

public class EtsGameDataProvider : IGameDataProvider<SCSTelemetry>
{
    private readonly TelemetryCapture _capture;
    private SCSSdkTelemetry? _telemetry;

    public EtsGameDataProvider(TelemetryCapture capture)
    {
        _capture = capture;
    }

    public event EventHandler<SCSTelemetry>? DataReceived;

    public void Start()
    {
        _telemetry = new SCSSdkTelemetry();
        if (_capture.Enabled)
        {
            _telemetry.RawDataReceived += HandleRawData;
        }
        _telemetry.Data += HandleTelemetryData;
    }

    public void Stop()
    {
        if (_telemetry is null)
        {
            return;
        }

        _telemetry.Data -= HandleTelemetryData;
        _telemetry.RawDataReceived -= HandleRawData;
        _telemetry.Dispose();
        _telemetry = null;
    }

    private void HandleRawData(byte[] bytes) =>
        _capture.RecordRawFrame(
            DisplayDefinitions.Game.Ets.Slug,
            "shared-memory",
            "Local\\SCSTelemetry",
            new Dictionary<string, byte[]> { ["data"] = bytes });

    private void HandleTelemetryData(SCSTelemetry data, bool newTimestamp)
    {
        DataReceived?.Invoke(this, data);
    }
}
