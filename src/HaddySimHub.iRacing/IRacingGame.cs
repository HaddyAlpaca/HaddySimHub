using HaddySimHub.GameData;
using HaddySimHub.Logging;

public sealed class IRacingGame : Game
{
    public IRacingGame(IProcessMonitor processMonitor, ILogger logger, CancellationToken cancellationToken)
        : base(processMonitor, logger, cancellationToken)
    {
        iRacingSDK.iRacing.NewData += this.ProcessData;

        this.Started += (s, e) => {
            iRacingSDK.iRacing.StartListening();
        };

        this.Stopped += (s, e) => {
            iRacingSDK.iRacing.StopListening();
        };
    }

    public override string Description => "IRacing";

    protected override string ProcessName => "iracingui";

    protected override IDisplay CurrentDisplay => new DashboardDisplay();
}