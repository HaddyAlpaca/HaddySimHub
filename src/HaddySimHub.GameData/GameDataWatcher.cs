using HaddySimHub.GameData.Models;

namespace HaddySimHub.GameData;

public interface IGameDataWatcher
{
    event EventHandler? GameDataIdle;
    event EventHandler<object>? RawDataUpdated;
    event EventHandler<TruckData>? TruckDataUpdated;
    event EventHandler<RaceData>? RaceDataUpdated;
    void Start(bool emitRawData, CancellationToken cancellationToken);
}

public class GameDataWatcher(
    IEnumerable<IGameDataReader> readers,
    IProcessMonitor processMonitor) : IGameDataWatcher
{
    private readonly IEnumerable<IGameDataReader> readers = readers;
    private readonly IProcessMonitor processMonitor = processMonitor;
    private Timer? _processTimer;
    private Timer? _dataTimer;
    private IGameDataReader? _gameDataReader;

    public event EventHandler? GameDataIdle;

    public event EventHandler<object>? RawDataUpdated;
    public event EventHandler<TruckData>? TruckDataUpdated;
    public event EventHandler<RaceData>? RaceDataUpdated;

    public void Start(bool emitRawData, CancellationToken cancellationToken)
    {
        //Monitor processes every 10 seconds
        this._processTimer = new Timer(_ =>
        {
            //Get the process that is running
            IGameDataReader? currentReader = readers.FirstOrDefault(reader => processMonitor.IsRunning(reader.ProcessName));

            if (this._gameDataReader != currentReader)
            {
                this._gameDataReader = currentReader;
            }
        }, cancellationToken, TimeSpan.Zero, TimeSpan.FromSeconds(10));

        //Monitor game data at approx. 60Hz
        this._dataTimer = new Timer(_ =>
        {
            //Read the current game data
            if (this._gameDataReader != null)
            {
                //A game is active
                if (emitRawData)
                {
                    var data = this._gameDataReader.ReadRawData();
                    this.RawDataUpdated?.Invoke(this, data);
                }
                else
                {
                    //Read the data from the game
                    var data = this._gameDataReader.ReadData();
                    if (data is RaceData raceData)
                    {
                        this.RaceDataUpdated?.Invoke(this, raceData);
                    }
                    else if (data is TruckData truckData)
                    {
                        this.TruckDataUpdated?.Invoke(this, truckData);
                    }
                }
            }
            else
            {
                //No game is active
                this.GameDataIdle?.Invoke(this, new EventArgs());
            }
        }, cancellationToken, TimeSpan.Zero, TimeSpan.FromMilliseconds(16));
    }
}
