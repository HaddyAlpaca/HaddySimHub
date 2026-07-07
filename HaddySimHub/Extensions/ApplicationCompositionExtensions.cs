using HaddySimHub.Displays;
using HaddySimHub.Interfaces;
using HaddySimHub.Models;
using HaddySimHub.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.Threading.Channels;

namespace HaddySimHub.Extensions;

public static class ApplicationCompositionExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public static IServiceCollection AddHaddySimHubApplication(this IServiceCollection services)
    {
        services.AddCors(corsOptions =>
        {
            corsOptions.AddDefaultPolicy(policyBuilder =>
            {
                policyBuilder
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });
        services.AddControllers();

        services.AddSingleton<IUdpClientFactory, UdpClientFactory>();
        services.AddSingleton<ISCSTelemetryFactory, SCSSdkTelemetryFactory>();
        services.AddSingleton<IDisplayFactory, DisplayFactory>();
        services.AddSingleton<ISseBroadcastService, SseBroadcastService>();
        services.AddSingleton<IDisplayUpdateSender, DisplayUpdateSender>();

        services.RegisterGameDisplay<Displays.Dirt2.Dirt2GameDataProvider, Displays.Dirt2.Dirt2DataConverter, Displays.Dirt2.Packet>(DisplayDefinitions.Game.Dirt2);
        services.RegisterGameDisplay<Displays.ETS.EtsGameDataProvider, Displays.ETS.EtsDataConverter, SCSSdkClient.Object.SCSTelemetry>(DisplayDefinitions.Game.Ets);
        services.RegisterGameDisplay<Displays.IRacing.IRacingGameDataProvider, Displays.IRacing.IRacingDataConverter, iRacingSDK.IDataSample>(DisplayDefinitions.Game.IRacing);
        services.RegisterGameDisplay<Displays.AC.ACGameDataProvider, Displays.AC.ACDataConverter, Displays.AC.ACTelemetry>(DisplayDefinitions.Game.Ac);
        services.RegisterGameDisplay<Displays.ACC.ACCGameDataProvider, Displays.ACC.ACCDataConverter, Displays.ACC.ACCTelemetry>(DisplayDefinitions.Game.Acc);
        services.RegisterGameDisplay<Displays.ACRally.ACRallyGameDataProvider, Displays.ACRally.ACRallyDataConverter, Displays.ACRally.ACRallyTelemetry>(DisplayDefinitions.Game.AcRally);

        services.AddSingleton<IDataConverter<DisplayUpdate, DisplayUpdate>, IdentityDataConverter<DisplayUpdate>>();
        services.RegisterTestDisplay<Displays.IRacing.TestDisplay>(DisplayDefinitions.TestIds.Race);
        services.RegisterTestDisplay<Displays.Dirt2.TestDisplay>(DisplayDefinitions.TestIds.Rally);
        services.RegisterTestDisplay<Displays.ETS.TestDisplay>(DisplayDefinitions.TestIds.Truck);

        services.AddSingleton<DisplaysRunner>();
        services.AddHostedService<DisplayRunnerHostedService>();
        services.AddHostedService<Dashboard.ConsoleDashboardHostedService>();

        return services;
    }

    public static WebApplication ConfigureHaddySimHubPipeline(this WebApplication app)
    {
        app.UseRouting();
        app.UseDefaultFiles();
        app.UseStaticFiles();
        app.UseCors();

        app.MapGet("/display-data/stream", async (HttpContext context, ISseBroadcastService broadcastService) =>
        {
            context.Response.ContentType = "text/event-stream";
            context.Response.Headers["Cache-Control"] = "no-cache";
            context.Response.Headers["X-Accel-Buffering"] = "no";

            var channel = Channel.CreateBounded<DisplayUpdate>(new BoundedChannelOptions(10)
            {
                FullMode = BoundedChannelFullMode.DropOldest,
            });

            broadcastService.SetClient(channel.Writer);

            try
            {
                await context.Response.WriteAsync("event: connected\ndata: {}\n\n", context.RequestAborted);
                await context.Response.Body.FlushAsync(context.RequestAborted);

                await foreach (var update in channel.Reader.ReadAllAsync(context.RequestAborted))
                {
                    var json = JsonSerializer.Serialize(update, JsonOptions);
                    await context.Response.WriteAsync($"data: {json}\n\n", context.RequestAborted);
                    await context.Response.Body.FlushAsync(context.RequestAborted);
                }
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                broadcastService.ClearClient();
                channel.Writer.TryComplete();
            }
        });

        return app;
    }
}
