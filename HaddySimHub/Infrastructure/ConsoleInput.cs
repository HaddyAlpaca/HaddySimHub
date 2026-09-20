namespace HaddySimHub.Infrastructure;

public static class ConsoleInput
{
    public static Task RunAsync(CancellationToken cancellationToken)
    {
        return Task.Run(async () =>
        {
            if (Console.IsInputRedirected)
            {
                return;
            }

            while (!cancellationToken.IsCancellationRequested)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(intercept: true);
                    if (key.Modifiers == ConsoleModifiers.Control && key.Key == ConsoleKey.T)
                    {
                        TestModeController.Cycle();
                    }
                }

                await Task.Delay(100, cancellationToken);
            }
        }, cancellationToken);
    }
}
