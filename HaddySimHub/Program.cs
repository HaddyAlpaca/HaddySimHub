using HaddySimHub.Infrastructure;
using HaddySimHub;

public class Program
{
    public static string TestId
    {
        get => TestModeController.Current;
        private set => TestModeController.Set(value);
    }

    public static async Task Main(string[] args)
    {
        Logger.Setup();
        SingleInstanceGuard.StopOtherInstances();

        TestModeController.ApplyArgument(args);

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

        var keyInputTask = ConsoleInput.RunAsync(token);
        var webServer = WebServerHost.Create();

        await WebServerHost.RunAsync(webServer, token);

        await cancellationTokenSource.CancelAsync();

        try
        {
            await keyInputTask;
        }
        catch (OperationCanceledException)
        {
        }
    }
}
