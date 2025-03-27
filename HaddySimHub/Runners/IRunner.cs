namespace HaddySimHub.Runners;

internal interface IRunner
{
    Task RunAsync(CancellationToken cancellationToken);
}