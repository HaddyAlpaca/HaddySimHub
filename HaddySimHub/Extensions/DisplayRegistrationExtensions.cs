using Microsoft.Extensions.DependencyInjection;
using HaddySimHub.Interfaces;
using HaddySimHub.Displays;
using HaddySimHub.Models;

namespace HaddySimHub.Extensions;

/// <summary>
/// Extension methods for registering game displays and related services with dependency injection.
/// </summary>
public static class DisplayRegistrationExtensions
{
    /// <summary>
    /// Registers a game display with its associated provider and converter.
    /// </summary>
    /// <typeparam name="TProvider">The game data provider type</typeparam>
    /// <typeparam name="TConverter">The converter implementation type.</typeparam>
    /// <typeparam name="TInput">The telemetry input type for the converter.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="definition">Typed display definition.</param>
    public static IServiceCollection RegisterGameDisplay<TProvider, TConverter, TInput>(
        this IServiceCollection services,
        GameDisplayDefinition<TInput> definition)
        where TProvider : class, IGameDataProvider<TInput>
        where TConverter : class, IDataConverter<TInput, DisplayUpdate>
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(definition);

        // Register the provider
        services.AddSingleton<IGameDataProvider<TInput>, TProvider>();

        // Register the converter
        services.AddSingleton<IDataConverter<TInput, DisplayUpdate>, TConverter>();

        services.AddSingleton<IDisplay>(sp => CreateGameDisplay(sp, definition));

        return services;
    }

    public static IDisplay CreateGameDisplay<TInput>(
        IServiceProvider serviceProvider,
        GameDisplayDefinition<TInput> definition)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        ArgumentNullException.ThrowIfNull(definition);
        if (string.IsNullOrWhiteSpace(definition.ProcessName))
        {
            throw new ArgumentException("Process name cannot be empty.", nameof(definition));
        }
        if (string.IsNullOrWhiteSpace(definition.Description))
        {
            throw new ArgumentException("Description cannot be empty.", nameof(definition));
        }

        var provider = serviceProvider.GetRequiredService<IGameDataProvider<TInput>>();
        var converter = serviceProvider.GetRequiredService<IDataConverter<TInput, DisplayUpdate>>();
        var sender = serviceProvider.GetRequiredService<IDisplayUpdateSender>();

        return new SimpleGameDisplay<TInput>(definition.ProcessName, definition.Description, provider, converter, sender);
    }
}
