using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;

namespace HaddySimHub.Displays
{
    /// <summary>
    /// Default implementation of IDisplayFactory. Creates display instances based on type name.
    /// Supports both simple displays and parameterized test displays with IDs.
    /// </summary>
    public class DisplayFactory : IDisplayFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IOptions<TestDisplayOptions> _testDisplayOptions;

        public DisplayFactory(IServiceProvider serviceProvider, IOptions<TestDisplayOptions> testDisplayOptions)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _testDisplayOptions = testDisplayOptions ?? throw new ArgumentNullException(nameof(testDisplayOptions));
        }

        public IDisplay Create(string displayTypeName)
        {
            return displayTypeName switch
            {
                "Dirt2.Display" => new Displays.Dirt2.Display(_serviceProvider.GetRequiredService<IUdpClientFactory>()),
                "Dirt2.TestDisplay" => new Displays.Dirt2.TestDisplay("rally"),
                "IRacing.Display" => new Displays.IRacing.Display(),
                "IRacing.TestDisplay" => new Displays.IRacing.TestDisplay("race"),
                "ETS.Display" => new Displays.ETS.Display(_serviceProvider.GetRequiredService<ISCSTelemetryFactory>()),
                "ETS.TestDisplay" => new Displays.ETS.TestDisplay("truck"),
                _ => throw new InvalidOperationException($"Unknown display type: {displayTypeName}")
            };
        }
    }
}
