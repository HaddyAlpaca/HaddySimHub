using Microsoft.Extensions.Hosting;
using HaddySimHub;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using HaddySimHub.Runners;
using Logger = HaddySimHub.Logger;

public class Program
{
    public static async Task Main(string[] args)
    {
        VerifySingleInstance();
        Logger.Setup(args.Contains("--debug"));

        if (!args.Contains("--no-update"))
        {
            await CheckForUpdates();
        }

        using CancellationTokenSource cancellationTokenSource = new();
        CancellationToken token = cancellationTokenSource.Token;

        Console.CancelKeyPress += (sender, eventArgs) =>
        {
            eventArgs.Cancel = true;
            cancellationTokenSource.Cancel();
            Logger.Info("Ctrl+C pressed. Exiting application...");
        };

        WebApplication webServer = CreateWebServer();

        Task webServerTask = RunWebServerAsync(webServer, token);
        Task processTask = RunProcessAsync(args, token);
        await Task.WhenAll(webServerTask, processTask);

        cancellationTokenSource.Cancel();
    }

    private static WebApplication CreateWebServer()
    {
        WebApplicationOptions options = new() { ContentRootPath = AppContext.BaseDirectory };
        var builder = WebApplication.CreateBuilder(options);

        // Configure Kestrel to listen on port 3333 on any IP.
        builder.WebHost.UseKestrel(options => options.ListenAnyIP(3333));

        // Configure CORS, controllers, and SignalR.
        builder.Services.AddCors(corsOptions =>
        {
            corsOptions.AddDefaultPolicy(policyBuilder =>
            {
                policyBuilder
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });
        builder.Services.AddControllers();
        builder.Services.AddSignalR(options => options.EnableDetailedErrors = true);

        var app = builder.Build();
        app.UseRouting();
        app.UseDefaultFiles();
        app.UseStaticFiles();
        app.UseCors();
        app.MapHub<GameDataHub>("/display-data");

        return app;
    }

    private static async Task RunWebServerAsync(WebApplication webServer, CancellationToken token)
    {
        try
        {
            await webServer.StartAsync(token);
            await webServer.WaitForShutdownAsync(token);
        }
        catch (OperationCanceledException)
        {
            // Expected cancellation, no action required.
        }
        catch (Exception ex)
        {
            Logger.Fatal($"Web server error: {ex.Message}\n\n{ex.StackTrace}");
        }
    }

    private static async Task RunProcessAsync(string[] args, CancellationToken token)
    {
        try
        {
            string? testRunnerArg = args.FirstOrDefault(arg => arg.StartsWith("--test-runner:", StringComparison.OrdinalIgnoreCase));
            string? testRunnerName = testRunnerArg?.Split(':')?.Last();
            IRunner? runner = testRunnerName?.ToLower() switch
            {
                "truck" => new TruckTestRunner(),
                "race" => new RaceTestRunner(),
                "rally" => new RallyTestRunner(),
                _ => testRunnerArg is null ? new DisplaysRunner() : null,
            };

            if (runner is null)
            {
                Logger.Error(testRunnerName is null 
                    ? $"Argument '{testRunnerArg}' is invalid. Expected format: '--test-runner:<name>'."
                    : $"Value '{testRunnerName}' for argument '--test-runner:' is invalid.");

                Exit(1);
                return;
            }

            Logger.Info($"Starting process runner: {runner.GetType().Name}");

            await runner.RunAsync(token);
        }
        catch (OperationCanceledException)
        {
            // Expected cancellation, no action required.
        }
        catch (Exception ex)
        {
            Logger.Fatal($"Process runner error: {ex.Message}\n\n{ex.StackTrace}");
        }
    }

    private static void VerifySingleInstance()
    {
        using Mutex mutex = new(true, "HaddySimHub_SingleInstance", out bool createdNew);
        if (!createdNew)
        {
            Logger.Error("Another instance of HaddySimHub is already running.");
            Exit(1);
        }
    }

    private static async Task CheckForUpdates()
    {
        try
        {
            if (await Updater.UpdateAvailable())
            {
                Logger.Info("Update available. Starting updater...");
                Updater.Update();
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"Error checking for updates: {ex.Message}\n\n{ex.StackTrace}");
        }
    }

    private static void Exit(int exitCode)
    {
        Logger.Info($"Exiting with code {exitCode}.");
        Environment.Exit(exitCode);
    }
}
