using iRacingSDK.Support;

namespace iRacingSDK;

public class iRacingEvents : IDisposable
{
    readonly iRacingConnection instance = new iRacingConnection();
    readonly CrossThreadEvents<DataSample> newData = new CrossThreadEvents<DataSample>();
    readonly CrossThreadEvents<DataSample> newSessionData = new CrossThreadEvents<DataSample>();
    readonly CrossThreadEvents connected = new CrossThreadEvents();
    readonly CrossThreadEvents disconnected = new CrossThreadEvents();
    readonly TimeSpan period;

    Task backListener;
    bool requestCancel;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="period">The time interval for raising the NewData event</param>
    public iRacingEvents(TimeSpan period)
    {
        this.period = period;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="periodInMilliseconds">The time interval in milliseconds to raise the NewData Event.  0 means as often as data arrives from iRacing.</param>
    public iRacingEvents(int periodInMilliseconds = 0)
    {
        period = new TimeSpan(0, 0, 0, 0, periodInMilliseconds);
    }

    public event Action Connected
    {
        add { connected.Event += value; }
        remove { connected.Event -= value; }
    }

    public event Action Disconnected
    {
        add { disconnected.Event += value; }
        remove { disconnected.Event -= value; }
    }

    public event Action<DataSample> NewData
    {
        add { newData.Event += value; }
        remove { newData.Event -= value; }
    }

    public event Action<DataSample> NewSessionData
    {
        add { newSessionData.Event += value; }
        remove { newSessionData.Event -= value; }
    }

    public void StartListening()
    {
        if( backListener != null )
            throw new Exception("Already listening to iRacing data");

        requestCancel = false;
        
        backListener = new Task(Listen);
        backListener.Start();
    }

    public void StopListening()
    {
        var bl = backListener;

        if (backListener == null)
            throw new Exception("Not currently listening to iRacing data");

        requestCancel = true;
        bl.Wait(500);
    }

    void Listen()
    {
        var isConnected = false;
        var isDisconnected = true;
        var lastSessionInfoUpdate = -1;

        var periodCount = this.period;
        var lastTimeStamp = DateTime.Now;

        try
        {
            foreach (var d in instance.GetDataFeed(logging: false))
            {
                if (requestCancel)
                    return;

                if (!isConnected && d.IsConnected)
                {
                    isConnected = true;
                    isDisconnected = false;
                    connected.Invoke();
                }

                if (!isDisconnected && !d.IsConnected)
                {
                    isConnected = false;
                    isDisconnected = true;
                    disconnected.Invoke();
                }

                if (period >= (DateTime.Now - lastTimeStamp))
                    continue;

                lastTimeStamp = DateTime.Now;

                if ( d.IsConnected)
                    newData.Invoke(d);

                if (d.IsConnected && d.SessionData.InfoUpdate != lastSessionInfoUpdate)
                {
                    lastSessionInfoUpdate = d.SessionData.InfoUpdate;
                    newSessionData.Invoke(d);
                }
            }
        }
        catch(Exception e)
        {
            TraceError.WriteLine(e.Message);
            TraceError.WriteLine(e.StackTrace);
        }
        finally
        {
            backListener = null;
        }
    }

    public void Dispose()
    {
        StopListening();
    }
}
