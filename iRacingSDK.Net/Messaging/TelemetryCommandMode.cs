namespace iRacingSDK;

public enum TelemetryCommandMode
{
    /// <summary>
    /// Turn telemetry recording off
    /// </summary>
    Stop = 0,
    /// <summary>
    /// Turn telemetry recording on
    /// </summary>
    Start,
    /// <summary>
    /// Write current file to disk and start a new one
    /// </summary>
    Restart
};
