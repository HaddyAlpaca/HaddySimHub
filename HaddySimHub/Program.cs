using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using HaddySimHub;
using HaddySimHub.Dashboard;
using HaddySimHub.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Logger = HaddySimHub.Logger;

public class Program
{
    public static string TestId { get; private set; } = string.Empty;

    public static async Task Main(string[] args)
    {
        VerifySingleInstance();
        Logger.Setup();

        if (!args.Contains("--no-update"))
        {
            await CheckForUpdates();
        }

        using CancellationTokenSource cancellationTokenSource = new();
        CancellationToken token = cancellationTokenSource.Token;

        Console.CancelKeyPress += (sender, eventArgs) =>
        {
            // Cancel cooperatively so the web host (and the live dashboard it
            // hosts) can shut down gracefully and restore the terminal.
            eventArgs.Cancel = true;
            Logger.Info("Ctrl+C pressed. Exiting application...");
            cancellationTokenSource.Cancel();
        };
        
        var keyInputTask = Task.Run(async () =>
        {
            if (Console.IsInputRedirected)
            {
                return;
            }

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

                        Logger.Info(string.IsNullOrEmpty(TestId) ? "Test mode disabled." : $"Test mode:'{TestId}'.");
                    }
                }

                await Task.Delay(100);
            }
        });

        WebApplication webServer = CreateWebServer();

        Task webServerTask = RunWebServerAsync(webServer, token);
        await Task.WhenAll(webServerTask, keyInputTask);

        cancellationTokenSource.Cancel();
    }

    private static WebApplication CreateWebServer()
    {
        WebApplicationOptions options = new() { ContentRootPath = AppContext.BaseDirectory };
        var builder = WebApplication.CreateBuilder(options);

        builder.WebHost.UseKestrel(options => options.ListenAnyIP(3333));

        // Avoid the default ASP.NET console logger corrupting the live dashboard.
        if (ConsoleDashboard.IsSupported)
        {
            builder.Logging.ClearProviders();
        }

        builder.Services.AddHaddySimHubApplication();

        var app = builder.Build();
        return app.ConfigureHaddySimHubPipeline();
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
        finally
        {
            // Stop hosted services (including the live dashboard) so they can
            // restore the terminal before the process exits.
            await webServer.StopAsync(CancellationToken.None);
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
                try
                {
                    Logger.Info($"Sending close signal to existing process {process.ProcessName} (ID: {process.Id})...");
                    process.CloseMainWindow();
                    if (!process.WaitForExit(3000))
                    {
                        Logger.Info($"Process did not close gracefully, killing...");
                        process.Kill();
                        process.WaitForExit(2000);
                    }
                }
                catch (InvalidOperationException)
                {
                    Logger.Info($"Process {process.Id} already exited.");
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
