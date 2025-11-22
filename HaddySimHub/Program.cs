using Microsoft.Extensions.Hosting;
using HaddySimHub;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Logger = HaddySimHub.Logger;

public class Program
{
    private static DisplaysRunner? _displaysRunner;
    public static string TestId { get; private set; } = string.Empty;

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
            Environment.Exit(0);
        };
        
        var keyInputTask = Task.Run(async () =>
        {
            while (!token.IsCancellationRequested)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(intercept: true);
                    if (key.Modifiers == ConsoleModifiers.Control && key.Key == ConsoleKey.T)
                    {
                        if (string.IsNullOrEmpty(TestId))
                        {
                            TestId = "race";
                        }
                        else if (TestId == "race")
                        {
                            TestId = "rally";
                        }
                        else if (TestId == "rally")
                        {
                            TestId = "truck";
                        }
                        else
                        {
                            TestId = string.Empty;
                        }

                        Console.WriteLine(string.IsNullOrEmpty(TestId) ? "Test mode disabled." : $"Test mode:'{TestId}'.");
                    }
                }

                await Task.Delay(100);
            }
        });

        WebApplication webServer = CreateWebServer();

        Task webServerTask = RunWebServerAsync(webServer, token);
        Task processTask = RunProcessAsync(webServer, token);
        await Task.WhenAll(webServerTask, processTask, keyInputTask);

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

        // Register test display options (can be overridden from config)
        builder.Services.Configure<HaddySimHub.Displays.TestDisplayOptions>(options =>
        {
            options.Ids = new List<string> { "race", "rally", "truck" };
        });

        builder.Services.AddSingleton<HaddySimHub.Displays.IUdpClientFactory, HaddySimHub.Displays.UdpClientFactory>();
        builder.Services.AddSingleton<HaddySimHub.Displays.ISCSTelemetryFactory, HaddySimHub.Displays.SCSSdkTelemetryFactory>();
        builder.Services.AddSingleton<HaddySimHub.Displays.IDisplayFactory, HaddySimHub.Displays.DisplayFactory>();

        // Register new services for refactored architecture
        builder.Services.AddSingleton<HaddySimHub.Interfaces.IDisplayUpdateSender, HaddySimHub.Services.DisplayUpdateSender>();

        // Register GameDataProviders
        builder.Services.AddSingleton<HaddySimHub.Interfaces.IGameDataProvider<HaddySimHub.Displays.Dirt2.Packet>, HaddySimHub.Displays.Dirt2.Dirt2GameDataProvider>();
        builder.Services.AddSingleton<HaddySimHub.Interfaces.IGameDataProvider<SCSSdkClient.Object.SCSTelemetry>, HaddySimHub.Displays.ETS.EtsGameDataProvider>();
        builder.Services.AddSingleton<HaddySimHub.Interfaces.IGameDataProvider<iRacingSDK.IDataSample>, HaddySimHub.Displays.IRacing.IRacingGameDataProvider>();

        // Register DataConverters
        builder.Services.AddSingleton<HaddySimHub.Interfaces.IDataConverter<HaddySimHub.Displays.Dirt2.Packet, HaddySimHub.Models.DisplayUpdate>, HaddySimHub.Displays.Dirt2.Dirt2DataConverter>();
        builder.Services.AddSingleton<HaddySimHub.Interfaces.IDataConverter<SCSSdkClient.Object.SCSTelemetry, HaddySimHub.Models.DisplayUpdate>, HaddySimHub.Displays.ETS.EtsDataConverter>();
        builder.Services.AddSingleton<HaddySimHub.Interfaces.IDataConverter<iRacingSDK.IDataSample, HaddySimHub.Models.DisplayUpdate>, HaddySimHub.Displays.IRacing.IRacingDataConverter>();
        builder.Services.AddSingleton<HaddySimHub.Interfaces.IDataConverter<HaddySimHub.Models.DisplayUpdate, HaddySimHub.Models.DisplayUpdate>, HaddySimHub.Services.IdentityDataConverter<HaddySimHub.Models.DisplayUpdate>>();

        // Register displays and runner for DI
        builder.Services.AddSingleton<DisplaysRunner>();
        builder.Services.AddSingleton<HaddySimHub.Displays.Dirt2.Display>();
        builder.Services.AddSingleton<HaddySimHub.Displays.IDisplay>(sp => sp.GetRequiredService<HaddySimHub.Displays.Dirt2.Display>());
        builder.Services.AddSingleton<HaddySimHub.Displays.IRacing.Display>();
        builder.Services.AddSingleton<HaddySimHub.Displays.IDisplay>(sp => sp.GetRequiredService<HaddySimHub.Displays.IRacing.Display>());
        builder.Services.AddSingleton<HaddySimHub.Displays.ETS.Display>();
        builder.Services.AddSingleton<HaddySimHub.Displays.IDisplay>(sp => sp.GetRequiredService<HaddySimHub.Displays.ETS.Display>());

        // Register test displays via factory
        builder.Services.AddSingleton<HaddySimHub.Displays.IDisplay>(sp => sp.GetRequiredService<HaddySimHub.Displays.IDisplayFactory>().Create("Dirt2.TestDisplay"));
        builder.Services.AddSingleton<HaddySimHub.Displays.IDisplay>(sp => sp.GetRequiredService<HaddySimHub.Displays.IDisplayFactory>().Create("IRacing.TestDisplay"));
        builder.Services.AddSingleton<HaddySimHub.Displays.IDisplay>(sp => sp.GetRequiredService<HaddySimHub.Displays.IDisplayFactory>().Create("ETS.TestDisplay"));

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

    private static async Task RunProcessAsync(WebApplication webServer, CancellationToken token)
    {
        try
        {
            var displaysRunner = webServer.Services.GetRequiredService<DisplaysRunner>();
            _displaysRunner = displaysRunner;
            await _displaysRunner.RunAsync(token);
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
        var currentProcess = System.Diagnostics.Process.GetCurrentProcess();
        var processes = System.Diagnostics.Process.GetProcessesByName(currentProcess.ProcessName);

        foreach (var process in processes)
        {
            if (process.Id != currentProcess.Id)
            {
                Logger.Info($"Killing process {process.ProcessName} with ID {process.Id}.");
                process.Kill();
                process.WaitForExit(5000); // Wait for 5 seconds
                if (!process.HasExited)
                {
                    Console.WriteLine($"Process {process.ProcessName} (PID {process.Id}) did not exit in time...");
                }
            }
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
}
