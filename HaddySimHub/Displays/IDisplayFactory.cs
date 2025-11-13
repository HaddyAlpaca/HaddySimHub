namespace HaddySimHub.Displays
{
    /// <summary>
    /// Factory for creating display instances, particularly parameterized displays like test displays.
    /// </summary>
    public interface IDisplayFactory
    {
        /// <summary>
        /// Create a display by its type name (e.g., "Dirt2.Display", "IRacing.TestDisplay.race").
        /// </summary>
        IDisplay Create(string displayTypeName);
    }
}
