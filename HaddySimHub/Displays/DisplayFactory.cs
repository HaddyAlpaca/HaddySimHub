using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using HaddySimHub.Interfaces;
using HaddySimHub.Models;

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
                // Game displays using SimpleGameDisplay
                "Dirt2.Display" => CreateGameDisplay<Dirt2.Packet>(
                    "dirtrally2",
                    "Dirt Rally 2"),
                "IRacing.Display" => CreateGameDisplay<iRacingSDK.IDataSample>(
                    "iracingui",
                    "IRacing"),
                "ETS.Display" => CreateGameDisplay<SCSSdkClient.Object.SCSTelemetry>(
                    "eurotrucks2",
                    "Euro Truck Simulator 2"),
                "AC.Display" => CreateGameDisplay<AC.ACTelemetry>(
                    "ac",
                    "Assetto Corsa"),
                "ACC.Display" => CreateGameDisplay<ACC.ACCTelemetry>(
                    "ac2",
                    "Assetto Corsa Competizione"),
                "ACRally.Display" => CreateGameDisplay<ACRally.ACRallyTelemetry>(
                    "acr",
                    "Assetto Corsa Rally"),

                // Test displays
                "Dirt2.TestDisplay" => new Dirt2.TestDisplay(
                    "rally",
                    _serviceProvider.GetRequiredService<IDataConverter<DisplayUpdate, DisplayUpdate>>(),
                    _serviceProvider.GetRequiredService<IDisplayUpdateSender>()),
                "IRacing.TestDisplay" => new IRacing.TestDisplay(
                    "race",
                    _serviceProvider.GetRequiredService<IDataConverter<DisplayUpdate, DisplayUpdate>>(),
                    _serviceProvider.GetRequiredService<IDisplayUpdateSender>()),
                "ETS.TestDisplay" => new ETS.TestDisplay(
                    "truck",
                    _serviceProvider.GetRequiredService<IDataConverter<DisplayUpdate, DisplayUpdate>>(),
                    _serviceProvider.GetRequiredService<IDisplayUpdateSender>()),

                _ => throw new InvalidOperationException($"Unknown display type: {displayTypeName}")
            };
        }

        /// <summary>
        /// Generic helper to create a game display using SimpleGameDisplay<T>
        /// </summary>
        private IDisplay CreateGameDisplay<T>(string processName, string description)
        {
            var provider = _serviceProvider.GetRequiredService<IGameDataProvider<T>>();
            var converter = _serviceProvider.GetRequiredService<IDataConverter<T, DisplayUpdate>>();
            var sender = _serviceProvider.GetRequiredService<IDisplayUpdateSender>();

            return new SimpleGameDisplay<T>(processName, description, provider, converter, sender);
        }
    }
}

