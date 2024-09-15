public sealed class IRacingGame : Game
{
    public override void Start()
    {
        base.Start();

        iRacingSDK.iRacing.NewData += this.ProcessData;
        iRacingSDK.iRacing.StartListening();
    }

    public override void Stop()
    {
        base.Stop();

        iRacingSDK.iRacing.NewData -= this.ProcessData;
        iRacingSDK.iRacing.StopListening();
    }

    public override string Description => "IRacing";

    public override string ProcessName => "iracingui";

    protected override IDisplay CurrentDisplay => new DashboardDisplay();
}