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

                    if (key.Modifiers == ConsoleModifiers.Control && key.Key == ConsoleKey.PageUp)
                    {
                        _displaysRunner?.CurrentDisplay?.NextPage();
                    }
                }

                await Task.Delay(100);
            }
        });

        WebApplication webServer = CreateWebServer();

        Task webServerTask = RunWebServerAsync(webServer, token);
        Task requestMonitor = MonitorFolderAsync(Path.Combine(AppContext.BaseDirectory, "Requests"), token);
        Task processTask = RunProcessAsync(token);
        await Task.WhenAll(webServerTask, processTask, keyInputTask, requestMonitor);

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

    private static async Task RunProcessAsync(CancellationToken token)
    {
        try
        {
            _displaysRunner = new DisplaysRunner();
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

    private static async Task MonitorFolderAsync(string folderPath, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string[] files = Directory.GetFiles(folderPath);

            foreach (var file in files)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                try
                {
                    using (FileStream fs = new(file, FileMode.Open, FileAccess.Read, FileShare.None))
                    using (StreamReader reader = new(fs))
                    {
                        string content = await reader.ReadToEndAsync(cancellationToken);
                        if (content.Trim() == "PAGEUP")
                        {
                            Console.WriteLine("Execute next page command...");
                            _displaysRunner?.CurrentDisplay?.NextPage();
                        }
                    }

                    File.Delete(file);
                }
                catch (IOException)
                {
                    // File is likely in use, skip it
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing file '{file}': {ex.Message}");
                }
            }

            await Task.Delay(500, cancellationToken);
        }
    }
}
