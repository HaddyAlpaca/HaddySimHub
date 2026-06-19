using Microsoft.Extensions.DependencyInjection;
using HaddySimHub.Interfaces;
using HaddySimHub.Models;

namespace HaddySimHub.Displays
{
    /// <summary>
    /// Default implementation of IDisplayFactory.
    /// </summary>
    public class DisplayFactory : IDisplayFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public DisplayFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public IDisplay CreateGameDisplay<TTelemetry>(GameDisplayDefinition<TTelemetry> definition)
        {
            ArgumentNullException.ThrowIfNull(definition);
            if (string.IsNullOrWhiteSpace(definition.ProcessName))
            {
                throw new ArgumentException("Process name cannot be empty.", nameof(definition));
            }
            if (string.IsNullOrWhiteSpace(definition.Description))
            {
                throw new ArgumentException("Description cannot be empty.", nameof(definition));
            }

            var provider = _serviceProvider.GetRequiredService<IGameDataProvider<TTelemetry>>();
            var converter = _serviceProvider.GetRequiredService<IDataConverter<TTelemetry, DisplayUpdate>>();
            var sender = _serviceProvider.GetRequiredService<IDisplayUpdateSender>();

            return new SimpleGameDisplay<TTelemetry>(definition.ProcessName, definition.Description, provider, converter, sender);
        }

        public TDisplay CreateTestDisplay<TDisplay>(string id) where TDisplay : TestDisplayBase
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Test display id cannot be empty.", nameof(id));
            }
            return ActivatorUtilities.CreateInstance<TDisplay>(_serviceProvider, id);
        }
    }
}
