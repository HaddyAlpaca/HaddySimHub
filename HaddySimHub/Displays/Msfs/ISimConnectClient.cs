namespace HaddySimHub.Displays.Msfs;

/// <summary>
/// The seam between the MSFS provider and the native SimConnect API.
/// </summary>
/// <remarks>
/// Everything that touches <c>SimConnect.dll</c> lives behind this interface, in the
/// same spirit as <c>ISCSTelemetryFactory</c> and <c>IUdpClientFactory</c>, so the
/// provider can be tested without Windows or the simulator.
/// </remarks>
public interface ISimConnectClient : IDisposable
{
    bool IsConnected { get; }

    /// <summary>
    /// Attempts to open a connection and subscribe to the telemetry block. Safe to
    /// call repeatedly: it is the provider's retry mechanism while the game starts up.
    /// </summary>
    void Connect();

    void Disconnect();

    /// <summary>
    /// Drains the dispatch queue and returns the most recent telemetry block, if the
    /// sim sent one since the last call.
    /// </summary>
    bool TryReadTelemetry(out MsfsTelemetry telemetry);
}
