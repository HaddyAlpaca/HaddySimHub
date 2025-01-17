using BarRaider.SdTools;
using GregsStack.InputSimulatorStandard;
using HaddySimHub.Shared;
using Microsoft.AspNetCore.SignalR.Client;
using System.Text.Json;

namespace HaddyTruckSDPlugin
{
    public abstract class KeyActionBase : KeypadBase
    {
        private readonly HubConnection _hubConnection;
        private readonly InputSimulator _inputSimulator = new();
        private bool _signalRStarted = false;
        private string? _currentImage = null;

        protected TruckData _truckData = new();

        public KeyActionBase(ISDConnection connection, InitialPayload payload)
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
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    var data = element.Deserialize<TruckData>(options);
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
            string keys = this.GetActionKeys();
            if (!string.IsNullOrEmpty(keys))
            {
                this._inputSimulator.Keyboard.TextEntry(keys);
            }
        }

        public override void OnTick()
        {
            if (!this._signalRStarted)
            {
                try
                {
                    this._hubConnection.StartAsync().Wait();
                    this._signalRStarted = true;
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
            string image = this.GetStateImage();
            if (this._currentImage is null || this._currentImage != image)
            {
                this._currentImage = image;
                await this.SetImage(image);
            }
        }

        protected abstract string GetActionKeys();

        protected abstract string GetStateImage();

        private async Task SetImage(string imageFile) =>
            await Connection.SetImageAsync(Path.Combine("images", GetPluginId(), imageFile));

        private string GetPluginId()
        {
            // Get the PluginActionId attribute value
            var attribute = (Attribute
                .GetCustomAttributes(this.GetType(), typeof(PluginActionIdAttribute))
                .FirstOrDefault()) as PluginActionIdAttribute;

            return attribute?.ActionId.Split('.').Last() ?? string.Empty;
        }
    }
}
