using HaddySimHub.GameData;
using HaddySimHub.Logging;
using F12020Telemetry;

namespace HaddySimHub.DirtRally2;

public sealed class Dirt2Game : Game
{
    private readonly F12020TelemetryClient _client;

    public Dirt2Game(IProcessMonitor processMonitor, ILogger logger, CancellationToken cancellationToken)
    : base(processMonitor, logger, cancellationToken)
    {
        this._client = new F12020TelemetryClient(20777);
        this._client.OnCarTelemetryDataReceive += TelemetryUpdate;
    }

    public override string Description => "Dirt Rally 2";

    protected override string ProcessName => "dirtrally2";

    protected override IDisplay CurrentDisplay => new DashboardDisplay();

    private void TelemetryUpdate(PacketCarTelemetryData packet)
    {
        // The player index.
        int playerIndex = packet.Header.playerCarIndex;

        // Player car telemetry.
        CarTelemetryData playerData = packet.carTelemetryData[playerIndex];

        this.ProcessData(playerData);

        // WriteLine($"Throttle: {playerData.throttle}");
        // WriteLine($"Brake: {playerData.brake}");
        // WriteLine($"Wheel: {playerData.steer}");
        // WriteLine($"Speed: {playerData.speed}");
        // WriteLine($"RPM: {playerData.engineRPM}");
        // WriteLine($"REV %: {playerData.revLightsPercent}");
        // WriteLine($"Gear: {playerData.gear} (suggested: {packet.suggestedGear})");
        // WriteLine($"DRS: {(playerData.drs == 1 ? "open" : "closed")}");
        // WriteLine($"Engine Temp: {playerData.engineTemperature}");
        // WriteLine($"Session Time: {TimeSpan.FromSeconds(packet.Header.sessionTime)}");
        // WriteLine($"Packet version: {packet.Header.packetVersion}");
        // WriteLine($"Game major version: {packet.Header.gameMajorVersion}");
        // WriteLine($"Game minor version: {packet.Header.gameMinorVersion}");
    }
}
