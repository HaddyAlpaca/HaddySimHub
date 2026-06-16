using Microsoft.Extensions.Hosting;

namespace HaddySimHub;

public sealed class DisplayRunnerHostedService : BackgroundService
{
    private readonly DisplaysRunner _displaysRunner;

    public DisplayRunnerHostedService(DisplaysRunner displaysRunner)
    {
        _displaysRunner = displaysRunner ?? throw new ArgumentNullException(nameof(displaysRunner));
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return _displaysRunner.RunAsync(stoppingToken);
    }
}
