using HaddySimHub.Shared;
using Microsoft.AspNetCore.SignalR.Client;
using System.Text.Json;

namespace HaddyTruckSDPlugin
{
    internal class SignalRConnection
    {
        private readonly HubConnection _hubConnection;
        private bool _started = false;

        public event EventHandler<TruckData>? DataUpdated;

        public SignalRConnection()
        {
            this._hubConnection = new HubConnectionBuilder()
                .WithUrl("http://localhost:3333/display-data")
                .WithAutomaticReconnect()
                .Build();

            this._hubConnection.On<DisplayUpdate>("displayUpdate", update =>
            {
                if (update.Type == DisplayType.TruckDashboard &&
                    update.Data is not null &&
                    update.Data is JsonElement element)
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    var data = element.Deserialize<TruckData>(options);
                    if (data is not null)
                    {
                        this.DataUpdated?.Invoke(this, data);
                    }
                }
            });
        }

        public void StartIfNeeded()
        {
            if (!this._started)
            {
                this._hubConnection.StartAsync().Wait();
                this._started = true;
            }
        }

        public void Stop() => this._hubConnection.StopAsync().Wait();
    }
}
