using BarRaider.SdTools;
using HaddySimHub.Shared;
using Microsoft.AspNetCore.SignalR.Client;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace HaddyTruckSDPlugin
{
    [PluginActionId("com.haddyalpaca.truck.toggle")]
    public class ToggleAction : KeypadBase
    {
        private readonly HubConnection _hubConnection;
        private readonly string _imagesFolder = "Images/parking-brake/";
        private bool _signalRStarted = false;
        private TruckData _truckData = new();
        private bool? _on;

        public ToggleAction(ISDConnection connection, InitialPayload payload)
            : base(connection, payload)
        {
            this._hubConnection = new HubConnectionBuilder()
                .WithUrl("http://localhost:3333/display-data")
                .WithAutomaticReconnect()
                .Build();

            this._hubConnection.On<DisplayUpdate>("displayUpdate", async update =>
            {
                if (update.Type == DisplayType.TruckDashboard &&
                    update.Data is not null &&
                    update.Data is JsonElement element)
                {
                    var data = element.Deserialize<TruckData>();
                    if (data is not null)
                    {
                        this._truckData = data;
                        await this.UpdateState();
                    }
                }
            });
        }

        public override void KeyPressed(KeyPayload payload) { }

        public override void KeyReleased(KeyPayload payload)
        {
            SendKeys.SendWait(" ");
        }

        public override void OnTick()
        {
            if (!this._signalRStarted)
            {
                try
                {
                    this._hubConnection.StartAsync().Wait();
                    this._signalRStarted = true;
                    Logger.Instance.LogMessage(TracingLevel.INFO, "SignalR started.");
                }
                catch (Exception ex)
                {
                    Logger.Instance.LogMessage(TracingLevel.ERROR, ex.Message);
                }
            }
        }

        public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload) { }

        public override void ReceivedSettings(ReceivedSettingsPayload payload) { }

        public override void Dispose()
        {
            try
            {
                this._hubConnection.StopAsync().Wait();
            }
            catch (Exception ex)
            {
                Logger.Instance.LogMessage(TracingLevel.ERROR, ex.Message);
            }

            GC.SuppressFinalize(this);
        }

        private async Task UpdateState()
        {
            bool on = this._truckData.ParkingBrakeOn;
            Logger.Instance.LogMessage(TracingLevel.INFO, "State: " + on);

            if (this._on is null || this._on != on)
            {
                this._on = on;
                await this.SetImage(on ? "on" : "off");
            }
        }

        private async Task SetImage(string imageFile) => 
            await Connection.SetImageAsync(Path.Combine(this._imagesFolder, imageFile));
    }
}