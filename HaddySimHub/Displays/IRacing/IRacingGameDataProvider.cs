using HaddySimHub.Capture;
using HaddySimHub.Displays;
using HaddySimHub.Interfaces;
using iRacingSDK;

namespace HaddySimHub.Displays.IRacing;

public class IRacingGameDataProvider : IGameDataProvider<IDataSample>
{
    private readonly TelemetryCapture _capture;

    public IRacingGameDataProvider(TelemetryCapture capture)
    {
        _capture = capture;
    }

    public event EventHandler<IDataSample>? DataReceived;

    public void Start()
    {
        if (_capture.Enabled)
        {
            iRacing.RawDataReceived += HandleRawData;
            iRacing.RawCaptureIncomplete += HandleCaptureIncomplete;
        }
        iRacing.NewData += HandleNewData;
        iRacing.StartListening();
    }

    public void Stop()
    {
        if (iRacing.IsConnected)
        {
            iRacing.StopListening();
        }
        iRacing.NewData -= HandleNewData;
        iRacing.RawDataReceived -= HandleRawData;
        iRacing.RawCaptureIncomplete -= HandleCaptureIncomplete;
    }

    private void HandleRawData(byte[] bytes) =>
        _capture.RecordRawFrame(
            DisplayDefinitions.Game.IRacing.Slug,
            "shared-memory",
            "Local\\IRSDKMemMapFileName",
            new Dictionary<string, byte[]> { ["data"] = bytes });

    private void HandleCaptureIncomplete(string reason) =>
        _capture.MarkIncomplete(reason);

    private void HandleNewData(IDataSample data)
    {
        DataReceived?.Invoke(this, data);
    }
}
