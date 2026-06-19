namespace HaddySimHub.Displays
{
    /// <summary>
    /// Factory for creating display instances.
    /// </summary>
    public interface IDisplayFactory
    {
        IDisplay CreateGameDisplay<TTelemetry>(GameDisplayDefinition<TTelemetry> definition);
        TDisplay CreateTestDisplay<TDisplay>(string id) where TDisplay : TestDisplayBase;
    }
}
