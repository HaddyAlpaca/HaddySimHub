namespace HaddySimHub.Displays;

public interface IDisplay
{
    string Description { get; }
    bool IsActive { get; }

    /// <summary>
    /// UTC timestamp of the last telemetry update pushed by this display, or
    /// <c>null</c> when no data has been received since it was last started.
    /// Used by the dashboard to distinguish "process running" from "data flowing".
    /// </summary>
    DateTime? LastUpdateUtc { get; }

    void Start();
    void Stop();
}
