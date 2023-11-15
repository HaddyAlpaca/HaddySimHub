using HaddySimHub.Telemetry;
using HaddySimHub.Telemetry.Models;
using HaddySimHub.Raceroom.Data;

namespace HaddySimHub.Raceroom;

public sealed class TelemetryReader : ITelemetryReader, IDisposable
{
    private readonly ISharedMemoryReader<Shared> mmf;
    public string ProcessName => "rrre";

    public TelemetryReader(ISharedMemoryReaderFactory sharedMemoryReaderFactory)
    {
        this.mmf = sharedMemoryReaderFactory.Create<Shared>("$R3E");
    }

    public object ReadRawData() => this.mmf.Read();

    public object ReadTelemetry()
    {
        Shared rawData = this.mmf.Read();

        return new RaceData
        {
            Speed = (int)MpsToKph(rawData.CarSpeed),
            Gear = rawData.Gear,
            Rpm = (int)RpsToRpm(rawData.EngineRps),
            RpmMax = (int)RpsToRpm(rawData.MaxEngineRps)
        };
    }

    public void Dispose() => this.mmf.Dispose();

    private static float RpsToRpm(float rps) => rps * (60 / (2 * (float)Math.PI));

    private static float MpsToKph(float mps) => mps * 3.6f;
}
