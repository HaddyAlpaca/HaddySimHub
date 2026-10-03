using HaddySimHub;
using HaddySimHub.Infrastructure;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        // Parsed first, so --help and a mistyped option never stop another
        // instance or reach the network.
        if (!CommandLineOptions.TryParse(args, out var options, out var error))
        {
            Console.Error.WriteLine(error);
            Console.Error.WriteLine(CommandLineOptions.Usage);
            return 1;
        }

        if (options.HelpRequested)
        {
            Console.WriteLine(CommandLineOptions.Usage);
            return 0;
        }

        Logger.Setup();

        if (options.CaptureDirectory is not null)
        {
            Logger.Info($"Capturing raw game source bytes to {options.CaptureDirectory}");
        }

        if (!options.E2eMode)
        {
            SingleInstanceGuard.StopOtherInstances();
        }

        if (!options.NoUpdate)
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

        var webServer = WebServerHost.Create(options.E2eMode, options.CaptureDirectory);

        await WebServerHost.RunAsync(webServer, token);

        return 0;
    }
}
