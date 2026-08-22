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
        // Set logging up first: replacing an already running instance is worth a
        // log line, and the checks involved report why a process was left alone.
        Logger.Setup();
        VerifySingleInstance();

        ApplyTestModeArgument(args);

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

                await Task.Delay(100, token);
            }
        }, token);

        WebApplication webServer = CreateWebServer();

        await RunWebServerAsync(webServer, token);

        // The web server owns the application lifetime. Cancelling here rather than
        // waiting on both tasks together means a web server that stops on its own —
        // a failure to bind the port, for instance — still releases the key loop
        // instead of leaving the process running with nothing left to do.
        await cancellationTokenSource.CancelAsync();

        try
        {
            await keyInputTask;
        }
        catch (OperationCanceledException)
        {
            // Expected: the key loop is cancelled as part of shutting down.
        }
    }

    private static void ApplyTestModeArgument(string[] args)
    {
        // Allow starting directly in a test mode, e.g. `--test race`, so the
        // mode does not have to be cycled through manually with Ctrl+T.
        var index = Array.FindIndex(args, a => a == "--test" || a == "--test-mode");
        string? value = null;

        if (index >= 0 && index + 1 < args.Length)
        {
            value = args[index + 1];
        }
        else
        {
            var inline = args.FirstOrDefault(a => a.StartsWith("--test=", StringComparison.Ordinal));
            if (inline is not null)
            {
                value = inline["--test=".Length..];
            }
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        var normalized = value.Trim().ToLowerInvariant();
        var validIds = new[]
        {
            HaddySimHub.Displays.DisplayDefinitions.TestIds.Race,
            HaddySimHub.Displays.DisplayDefinitions.TestIds.Rally,
            HaddySimHub.Displays.DisplayDefinitions.TestIds.Truck,
        };

        if (Array.IndexOf(validIds, normalized) < 0)
        {
            Logger.Warn($"Unknown test mode '{value}'. Valid values: {string.Join(", ", validIds)}.");
            return;
        }

        TestId = normalized;
        Logger.Info($"Test mode:'{TestId}' (set via command-line argument).");
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
        var currentPath = GetExecutablePath(currentProcess);

        foreach (var process in processes)
        {
            if (process.Id != currentProcess.Id && IsSameExecutable(process, currentPath))
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

    /// <summary>
    /// Matching on the process name alone is not enough to identify another copy of
    /// this application: launched through <c>dotnet HaddySimHub.dll</c> the process
    /// is named <c>dotnet</c>, and every unrelated .NET process would match. When the
    /// path of either process cannot be read, the process is left alone.
    /// </summary>
    private static bool IsSameExecutable(System.Diagnostics.Process process, string? currentPath)
    {
        if (currentPath is null)
        {
            return false;
        }

        var path = GetExecutablePath(process);
        return path is not null && string.Equals(path, currentPath, StringComparison.OrdinalIgnoreCase);
    }

    private static string? GetExecutablePath(System.Diagnostics.Process process)
    {
        try
        {
            return process.MainModule?.FileName;
        }
        catch (Exception ex)
        {
            // Reading another process's module list needs privileges we may not have.
            Logger.Debug($"Could not read the executable path of process {process.Id}: {ex.GetType().Name}");
            return null;
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
