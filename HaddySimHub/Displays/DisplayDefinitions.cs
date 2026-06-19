namespace HaddySimHub.Displays;

public static class DisplayDefinitions
{
    public static class Game
    {
        public static readonly GameDisplayDefinition<Displays.Dirt2.Packet> Dirt2 = new("dirtrally2", "Dirt Rally 2");
        public static readonly GameDisplayDefinition<SCSSdkClient.Object.SCSTelemetry> Ets = new("eurotrucks2", "Euro Truck Simulator 2");
        public static readonly GameDisplayDefinition<iRacingSDK.IDataSample> IRacing = new("iracingui", "IRacing");
        public static readonly GameDisplayDefinition<Displays.AC.ACTelemetry> Ac = new("ac", "Assetto Corsa");
        public static readonly GameDisplayDefinition<Displays.ACC.ACCTelemetry> Acc = new("ac2", "Assetto Corsa Competizione");
        public static readonly GameDisplayDefinition<Displays.ACRally.ACRallyTelemetry> AcRally = new("acr", "Assetto Corsa Rally");
    }

    public static class TestIds
    {
        public const string Rally = "rally";
        public const string Race = "race";
        public const string Truck = "truck";
    }
}
