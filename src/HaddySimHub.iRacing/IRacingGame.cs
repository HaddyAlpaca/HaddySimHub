using HaddySimHub.GameData;
using HaddySimHub.Logging;

public sealed class IRacingGame : Game
{
    public IRacingGame(IProcessMonitor processMonitor, ILogger logger, CancellationToken cancellationToken)
        : base(processMonitor, logger, cancellationToken)
    {
        iRacingSDK.iRacing.NewData += this.ProcessData;
        iRacingSDK.iRacing.StartListening();
    }

    public override string Description => "IRacing";

    protected override string ProcessName => "iracingui";

    protected override IDisplay CurrentDisplay => new DashboardDisplay();
}