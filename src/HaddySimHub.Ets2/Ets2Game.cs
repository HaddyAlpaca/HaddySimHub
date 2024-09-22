using HaddySimHub.GameData;
using SCSSdkClient;
using SCSSdkClient.Object;

public sealed class Ets2Game : Game
{
    private SCSSdkTelemetry? telemetry;
    private SCSTelemetry? lastReceivedData;
    private List<string> _messages = new();

    public override void Start() {
        base.Start();

        this.telemetry = new ();
        this.telemetry.Data += (SCSTelemetry data, bool newTimestamp) =>
        {
            this.lastReceivedData = data;
            this.ProcessData(data);
        };

        this.telemetry.Tollgate += (s, e) =>
            this.AddMessage($"Tol betaald: {this.lastReceivedData?.GamePlay.TollgateEvent.PayAmount:C0}");

        this.telemetry.RefuelPayed += (s, e) =>
            this.AddMessage($"Brandstof betaald: {this.lastReceivedData?.GamePlay.RefuelEvent.Amount:C0}");

        this.telemetry.Ferry += (s, e) =>
            this.AddMessage(
                $"Bootreis gestart: {this.lastReceivedData?.GamePlay.FerryEvent.SourceName}" +
                $" - {this.lastReceivedData?.GamePlay.FerryEvent.TargetName} " +
                $"({this.lastReceivedData?.GamePlay.FerryEvent.PayAmount:C0})");

        this.telemetry.Train += (s, e) =>
            this.AddMessage($"Treinreis gestart: {this.lastReceivedData?.GamePlay.TrainEvent.SourceName} - {this.lastReceivedData?.GamePlay.TrainEvent.TargetName} ({this.lastReceivedData?.GamePlay.TrainEvent.PayAmount:C0})");

        this.telemetry.JobDelivered += (s, e) =>
            this.AddMessage($"Opdracht afgerond, opbrengst: {this.lastReceivedData?.GamePlay.JobDelivered.Revenue:C0}");

        this.telemetry.JobCancelled += (s, e) =>
            this.AddMessage($"Opdracht geannuleerd, boete: {this.lastReceivedData?.GamePlay.JobCancelled.Penalty:C0}");
    }

    public override void Stop()
    {
        base.Stop();

        this.telemetry?.Dispose();
        this.telemetry = null;
    }

    public override string Description => "Euro Truck Simulator 2";

    protected override string _processName => "eurotrucks2";

    protected override Func<object, DisplayUpdate> GetDisplayUpdate => Dashboard.GetDisplayUpdate;

    private void AddMessage(string message) => this._messages.Add(message);
}