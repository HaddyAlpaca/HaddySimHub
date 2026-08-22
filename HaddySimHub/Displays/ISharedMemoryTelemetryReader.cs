namespace HaddySimHub.Displays;

/// <summary>
/// Reads one game's telemetry from its shared memory pages.
/// </summary>
/// <typeparam name="TTelemetry">The flattened telemetry the game publishes.</typeparam>
public interface ISharedMemoryTelemetryReader<TTelemetry> : IDisposable
    where TTelemetry : struct
{
    /// <summary>True once the pages carrying live telemetry are mapped.</summary>
    bool IsConnected { get; }

    /// <summary>
    /// Maps whatever pages are available. Safe to call repeatedly: pages already
    /// mapped are left alone, and a game that is not running is not an error.
    /// </summary>
    void Connect();

    void Disconnect();

    /// <summary>
    /// Reads a frame. Returns false when the reader is not connected, or when the
    /// read failed and the reader disconnected itself so it can be retried.
    /// </summary>
    bool TryReadTelemetry(out TTelemetry telemetry);
}
