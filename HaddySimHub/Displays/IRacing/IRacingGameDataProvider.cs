using HaddySimHub.Interfaces;
using iRacingSDK;

namespace HaddySimHub.Displays.IRacing;

public class IRacingGameDataProvider : IGameDataProvider<IDataSample>
{
    public event EventHandler<IDataSample>? DataReceived;

    public IRacingGameDataProvider()
    {
    }

    public void Start()
    {
        iRacing.NewData += HandleNewData;
        iRacing.StartListening();
    }

    public void Stop()
    {
        if (iRacing.IsConnected)
        {
            iRacing.StopListening();
        }
        iRacing.NewData -= HandleNewData;
    }

    private void HandleNewData(IDataSample data)
    {
        DataReceived?.Invoke(this, data);
    }
}
