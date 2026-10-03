namespace HaddySimHub.Displays;

public static class DisplayDefinitions
{
    public static class Game
    {
        public static readonly GameDisplayDefinition<Displays.Dirt2.Packet> Dirt2 = new("dirtrally2", "Dirt Rally 2", "dirt2");
        public static readonly GameDisplayDefinition<SCSSdkClient.Object.SCSTelemetry> Ets = new("eurotrucks2", "Euro Truck Simulator 2", "ets2");
        public static readonly GameDisplayDefinition<iRacingSDK.IDataSample> IRacing = new("iracingui", "IRacing", "iracing");
        // The Assetto Corsa titles all publish their telemetry under the same shared
        // memory names, so the process is what tells them apart. See Displays/README.md.
        public static readonly GameDisplayDefinition<Displays.AC.ACTelemetry> Ac = new("acs", "Assetto Corsa", "ac");
        public static readonly GameDisplayDefinition<Displays.ACC.ACCTelemetry> Acc = new("AC2-Win64-Shipping", "Assetto Corsa Competizione", "acc");
        public static readonly GameDisplayDefinition<Displays.ACRally.ACRallyTelemetry> AcRally = new("acr", "Assetto Corsa Rally", "acrally");
        public static readonly GameDisplayDefinition<Displays.Msfs.MsfsTelemetry> Msfs = new("FlightSimulator", "Microsoft Flight Simulator 2020", "msfs");
        public static readonly GameDisplayDefinition<Displays.Forza.ForzaTelemetry> Forza = new("ForzaHorizon5", "Forza Horizon 5", "forza");
    }
}
