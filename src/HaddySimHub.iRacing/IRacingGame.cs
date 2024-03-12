using HaddySimHub.GameData;

public sealed class IRacingGame : Game
{
    public IRacingGame(IProcessMonitor processMonitor, CancellationToken cancellationToken)
        : base(processMonitor, cancellationToken)
    {
        iRacingSDK.iRacing.NewData += this.ProcessData;
        iRacingSDK.iRacing.StartListening();
    }

    public override string Description => "IRacing";

    protected override string ProcessName => "iracingui";

    protected override IDisplay CurrentDisplay => new DashboardDisplay();
}