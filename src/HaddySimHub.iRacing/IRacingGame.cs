using HaddySimHub.GameData;

public sealed class IRacingGame : Game
{
    public override void Start()
    {
        base.Start();

        iRacingSDK.iRacing.NewData += this.ProcessData;
        iRacingSDK.iRacing.StartListening();

        Task.Run(() => {
            foreach (var d in iRacingSDK.iRacing.GetDataFeed())
            {
                this._logger.Info($"Connected: {d.IsConnected}");
            }
        });
    }

    public override void Stop()
    {
        base.Stop();

        iRacingSDK.iRacing.NewData -= this.ProcessData;
        if (iRacingSDK.iRacing.IsConnected) {
            iRacingSDK.iRacing.StopListening();
        }
    }

    public override string Description => "IRacing";

    public override string ProcessName => "iracingui";

    protected override Func<object, DisplayUpdate> GetDisplayUpdate => Dashboard.GetDisplayUpdate;
}