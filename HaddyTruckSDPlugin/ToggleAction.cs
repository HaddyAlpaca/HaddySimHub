using BarRaider.SdTools;
using HaddySimHub.Shared;
using Microsoft.AspNetCore.SignalR.Client;

namespace HaddyTruckSDPlugin
{
    [PluginActionId("com.haddyalpaca.truck.toggle")]
    public class ToggleAction : KeypadBase
    {
        private readonly HubConnection _hubConnection;
        private bool _on;

        public ToggleAction(ISDConnection connection, InitialPayload payload)
            : base(connection, payload)
        {
            this._hubConnection = new HubConnectionBuilder()
                .WithUrl("http://localhost:3333/display-data")  
                .Build();

            this._hubConnection.On<DisplayUpdate>("displayUpdate", async update =>
            {
                if (update.Type == DisplayType.TruckDashboard)
                {
                    if (update.Data is TruckData data)
                    {
                        bool on = data.ParkingBrakeOn;
                        if (this._on != on)
                        {
                            this._on = on;

                            string imageFile = "Images/parking-braking/" + (this._on ? "on" : "off");

                            await Connection.SetImageAsync(imageFile);
                        }
                    }
                }
            });

            this._hubConnection.StartAsync().Wait();
        }

        public override void KeyPressed(KeyPayload payload) { }

        public override void KeyReleased(KeyPayload payload)
        {
            SendKeys.Send("Fiets");
        }

        public override void OnTick() { }

        public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload) { }

        public override void ReceivedSettings(ReceivedSettingsPayload payload) { }

        public override void Dispose()
        {
            this._hubConnection.StopAsync().Wait();
        }
    }
}