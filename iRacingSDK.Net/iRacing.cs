namespace iRacingSDK;

public static class iRacing
{
    private static iRacingConnection instance;
    private static iRacingEvents eventInstance;

    static iRacing()
    {
        instance = new iRacingConnection();
        eventInstance = new iRacingEvents();
    }
    public static PitCommand PitCommand => instance.PitCommand;

    public static bool IsConnected => instance.IsConnected;

    public static IEnumerable<DataSample> GetDataFeed() => instance.GetDataFeed();

    public static void StartListening() => eventInstance.StartListening();

    public static void StopListening() => eventInstance.StopListening();

    public static event Action<DataSample> NewData
    {
        add
        {
            eventInstance.NewData += value;
        }
        remove
        {
            eventInstance.NewData -= value;
        }
    }
}
