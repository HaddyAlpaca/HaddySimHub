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
    /// <typeparam name="TInput">The telemetry input type for the converter</typeparam>
    /// <typeparam name="TOutput">The display update output type (typically DisplayUpdate)</typeparam>
    /// <param name="services">The service collection</param>
    /// <param name="converterType">The data converter implementation type</param>
    /// <param name="displayFactoryName">The name used in the display factory (e.g., "AC.Display")</param>
    public static IServiceCollection RegisterGameDisplay<TProvider, TInput, TOutput>(
        this IServiceCollection services,
        Type converterType,
        string displayFactoryName)
        where TProvider : class, IGameDataProvider<TInput>
    {
        // Register the provider
        services.AddSingleton(typeof(IGameDataProvider<TInput>), typeof(TProvider));

        // Register the converter
        services.AddSingleton(typeof(IDataConverter<TInput, TOutput>), converterType);

        // Register the display via factory lookup
        services.AddSingleton<IDisplay>(sp =>
            sp.GetRequiredService<IDisplayFactory>().Create(displayFactoryName));

        return services;
    }
}
