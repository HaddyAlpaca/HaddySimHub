using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using HaddySimHub.Interfaces;
using HaddySimHub.Displays.Dirt2;
using HaddySimHub.Displays.IRacing;
using HaddySimHub.Displays.ETS;
using HaddySimHub.Displays.AC;
using HaddySimHub.Displays.ACC;
using HaddySimHub.Displays.ACRally;
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
                "Dirt2.Display" => new Displays.Dirt2.Display(
                    _serviceProvider.GetRequiredService<IGameDataProvider<Packet>>(),
                    _serviceProvider.GetRequiredService<IDataConverter<Packet, DisplayUpdate>>(),
                    _serviceProvider.GetRequiredService<IDisplayUpdateSender>()),
                "Dirt2.TestDisplay" => new Displays.Dirt2.TestDisplay(
                    "rally", 
                    _serviceProvider.GetRequiredService<IDataConverter<DisplayUpdate, DisplayUpdate>>(),
                    _serviceProvider.GetRequiredService<IDisplayUpdateSender>()),
                "IRacing.Display" => new Displays.IRacing.Display(
                    _serviceProvider.GetRequiredService<IGameDataProvider<iRacingSDK.IDataSample>>(),
                    _serviceProvider.GetRequiredService<IDataConverter<iRacingSDK.IDataSample, DisplayUpdate>>(),
                    _serviceProvider.GetRequiredService<IDisplayUpdateSender>()),
                "IRacing.TestDisplay" => new Displays.IRacing.TestDisplay(
                    "race",
                    _serviceProvider.GetRequiredService<IDataConverter<DisplayUpdate, DisplayUpdate>>(),
                    _serviceProvider.GetRequiredService<IDisplayUpdateSender>()),
                "ETS.Display" => new Displays.ETS.Display(
                    _serviceProvider.GetRequiredService<IGameDataProvider<SCSSdkClient.Object.SCSTelemetry>>(),
                    _serviceProvider.GetRequiredService<IDataConverter<SCSSdkClient.Object.SCSTelemetry, DisplayUpdate>>(),
                    _serviceProvider.GetRequiredService<IDisplayUpdateSender>()),
                "ETS.TestDisplay" => new Displays.ETS.TestDisplay(
                    "truck",
                    _serviceProvider.GetRequiredService<IDataConverter<DisplayUpdate, DisplayUpdate>>(),
                    _serviceProvider.GetRequiredService<IDisplayUpdateSender>()),
                "AC.Display" => new Displays.AC.Display(
                    _serviceProvider.GetRequiredService<IGameDataProvider<Displays.AC.ACTelemetry>>(),
                    _serviceProvider.GetRequiredService<IDataConverter<Displays.AC.ACTelemetry, DisplayUpdate>>(),
                    _serviceProvider.GetRequiredService<IDisplayUpdateSender>()),
                "ACC.Display" => new Displays.ACC.Display(
                    _serviceProvider.GetRequiredService<IGameDataProvider<Displays.ACC.ACCTelemetry>>(),
                    _serviceProvider.GetRequiredService<IDataConverter<Displays.ACC.ACCTelemetry, DisplayUpdate>>(),
                    _serviceProvider.GetRequiredService<IDisplayUpdateSender>()),
                "ACRally.Display" => new Displays.ACRally.Display(
                    _serviceProvider.GetRequiredService<IGameDataProvider<Displays.ACRally.ACRallyTelemetry>>(),
                    _serviceProvider.GetRequiredService<IDataConverter<Displays.ACRally.ACRallyTelemetry, DisplayUpdate>>(),
                    _serviceProvider.GetRequiredService<IDisplayUpdateSender>()),
                _ => throw new InvalidOperationException($"Unknown display type: {displayTypeName}")
            };
        }
    }
}
