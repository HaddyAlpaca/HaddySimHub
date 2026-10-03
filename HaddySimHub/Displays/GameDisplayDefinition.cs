namespace HaddySimHub.Displays;

/// <param name="ProcessName">Process the game runs under; also how the display decides it is active.</param>
/// <param name="Description">Human-readable game name.</param>
/// <param name="Slug">Short lowercase name, used for file names such as captured telemetry.</param>
public sealed record GameDisplayDefinition<TTelemetry>(string ProcessName, string Description, string Slug);
