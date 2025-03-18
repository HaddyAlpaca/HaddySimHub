namespace iRacingSDK;

public static partial class DataSampleExtensions
{
    /// <summary>
    /// Internal use in sdk only.
    /// Raise the connection and disconnection events as iRacing is started, stopped.
    /// </summary>
    internal static IEnumerable<DataSample> WithEvents(this IEnumerable<DataSample> samples, CrossThreadEvents connectionEvent, CrossThreadEvents disconnectionEvent, CrossThreadEvents<DataSample> newSessionData)
    {
        var isConnected = false;
        var isDisconnected = true;
        var lastSessionInfoUpdate = -1;

        foreach (var data in samples)
        {
            if (!isConnected && data.IsConnected)
            {
                isConnected = true;
                isDisconnected = false;
                connectionEvent.Invoke();
            }

            if (!isDisconnected && !data.IsConnected)
            {
                isConnected = false;
                isDisconnected = true;
                disconnectionEvent.Invoke();
            }

            if(data.IsConnected && data.SessionData.InfoUpdate != lastSessionInfoUpdate)
            {
                lastSessionInfoUpdate = data.SessionData.InfoUpdate;
                newSessionData.Invoke(data);
            }

            yield return data;
        }
    }
}
