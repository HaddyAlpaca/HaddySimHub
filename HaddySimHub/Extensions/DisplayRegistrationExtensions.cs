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

        // Register the display via typed factory creation
        services.AddSingleton<IDisplay>(sp =>
            sp.GetRequiredService<IDisplayFactory>().CreateGameDisplay(definition));

        return services;
    }

    public static IServiceCollection RegisterTestDisplay<TDisplay>(
        this IServiceCollection services,
        string id)
        where TDisplay : TestDisplayBase
    {
        ArgumentNullException.ThrowIfNull(services);
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Test display id cannot be empty.", nameof(id));
        }

        services.AddSingleton<IDisplay>(sp =>
            sp.GetRequiredService<IDisplayFactory>().CreateTestDisplay<TDisplay>(id));

        return services;
    }
}
