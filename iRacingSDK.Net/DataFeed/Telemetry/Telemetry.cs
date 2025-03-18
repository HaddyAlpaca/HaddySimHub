using System.Text;
using iRacingSDK.Support;

namespace iRacingSDK;

public partial class Telemetry : Dictionary<string, object>
{
    public SessionData._SessionInfo._Sessions Session 
    {
        get 
        {
            if (SessionNum < 0 || SessionNum >= SessionData.SessionInfo.Sessions.Length)
                return null;

            return SessionData.SessionInfo.Sessions[SessionNum];
        }
    }

    public Car CamCar => Cars[CamCarIdx];

    CarArray cars;
    public CarArray Cars
    {
        get
        {
            if (cars != null)
                return cars;

            return cars = new CarArray(this);
        }
    }

    public CarDetails[] CarDetails { get { return Cars.Select(c => c.Details).ToArray(); } }

    public IEnumerable<Car> RaceCars => Cars.Where(c => !c.Details.IsPaceCar);

    public bool UnderPaceCar => this.CarIdxTrackSurface[0] == TrackLocation.OnTrack;

    public Dictionary<string, string> Descriptions { get; internal set; }

    public override string ToString()
    {
        var result = new StringBuilder();

        foreach (var kv in this)
        {
            var key = kv.Key;
            var description = (Descriptions != null && Descriptions.ContainsKey(key)) ? Descriptions[key] : "";
            var value = ConvertToSpecificType(key, kv.Value);

            var type = value.GetType().ToString();

            result.Append("TeleKey: | {0,-30} | {1,-30} | {2,30} | {3}\n".F(key, type, value, description));
        }

        return result.ToString();
    }

    private object ConvertToSpecificType(string key, object value)
    {
        return key switch
        {
            "SessionState" => (SessionState)(int)value,
            "SessionFlags" => (SessionFlags)(int)value,
            "EngineWarnings" => (EngineWarnings)(int)value,
            "CarIdxTrackSurface" => ((int[])value).Select(v => (TrackLocation)v).ToArray(),
            "DisplayUnits" => (DisplayUnits)(int)value,
            "WeatherType" => (WeatherType)(int)value,
            "Skies" => (Skies)(int)value,
            _ => value,
        };
    }
}
