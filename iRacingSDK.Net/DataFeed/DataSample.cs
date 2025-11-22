namespace iRacingSDK;

public interface IDataSample
{
    bool IsConnected { get; set; }
    SessionData SessionData { get; set; }
    Telemetry Telemetry { get; set; }
    DataSample LastSample { get; set; }
}


[Serializable]
public class DataSample : IDataSample
{
    private Telemetry telemetry;
    private SessionData sessionData;

    public static readonly DataSample YetToConnected = new DataSample { IsConnected = false };
    public DataSample LastSample { get; set; }

    public bool IsConnected { get; set; }
    public SessionData SessionData
    {
        get
        {
            if (!IsConnected)
                throw new Exception("Attempt to read session data before connection to iRacing");

            if (sessionData == null)
                throw new Exception("SessionData is null");

            return sessionData;
        }
        set 
        {
            sessionData = value;
        }
    }

    public Telemetry Telemetry
    {
        get
        {
            if (!IsConnected)
                throw new Exception("Attempt to read telemetry data before connection to iRacing");

            if (telemetry == null)
                throw new Exception("Telemetry is null");

            return telemetry;
        }
        set
        {
            telemetry = value;
        }
    }
    
    public Dictionary<string, string> TelemetryDescription { get; internal set; }
}
