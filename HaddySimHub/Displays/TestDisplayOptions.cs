using Microsoft.Extensions.Options;

namespace HaddySimHub.Displays
{
    /// <summary>
    /// Options for configuring test displays (e.g., IDs, enabled state).
    /// </summary>
    public class TestDisplayOptions
    {
        public const string SectionName = "TestDisplays";

        /// <summary>
        /// Ordered list of test display IDs to create (e.g., ["race", "rally", "truck"]).
        /// </summary>
        public List<string> Ids { get; set; } = new() { "race", "rally", "truck" };
    }
}
