using HaddySimHub;
using HaddySimHub.Infrastructure;

public class Program
{
    public static async Task Main(string[] args)
    {
        Logger.Setup();
        var e2eMode = args.Contains("--e2e", StringComparer.Ordinal);
        if (!e2eMode)
        {
            SingleInstanceGuard.StopOtherInstances();
        }

        if (!args.Contains("--no-update"))
        {
            await UpdateChecker.CheckAsync();
        }

        using CancellationTokenSource cancellationTokenSource = new();
        CancellationToken token = cancellationTokenSource.Token;

        Console.CancelKeyPress += (_, eventArgs) =>
        {
            eventArgs.Cancel = true;
            Logger.Info("Ctrl+C pressed. Exiting application...");
            cancellationTokenSource.Cancel();
        };

        var webServer = WebServerHost.Create(e2eMode);

        await WebServerHost.RunAsync(webServer, token);
    }
}
