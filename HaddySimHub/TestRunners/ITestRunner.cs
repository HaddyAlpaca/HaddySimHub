namespace HaddySimHub.TestRunners;

internal interface ITestRunner
{
    Task RunAsync(CancellationToken cancellationToken);
}