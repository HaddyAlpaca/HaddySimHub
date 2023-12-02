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
    Dictionary<string, Type> readers,
    IProcessMonitor processMonitor) : IGameDataWatcher
{
    private readonly Dictionary<string, Type> readers = readers;
    private readonly IProcessMonitor processMonitor = processMonitor;
    private string _currentGameProcess = string.Empty;
    private Timer? _processTimer;
    private IGameDataReader? _gameDataReader;
    
    public event EventHandler? GameDataIdle;

    public event EventHandler<object>? RawDataUpdated;
    public event EventHandler<TruckData>? TruckDataUpdated;
    public event EventHandler<RaceData>? RaceDataUpdated;

    public void Start(bool emitRawData, CancellationToken cancellationToken)
    {
        this._currentGameProcess = string.Empty;

        //Monitor processes every 10 seconds
        this._processTimer = new Timer(_ =>
        {
            //Get the process that is running
            var runningGameProcesses = readers.Where(r => processMonitor.IsRunning(r.Key));

            if (!runningGameProcesses.Any())
            {
                //No games running
                this._gameDataReader = null;
                this.GameDataIdle?.Invoke(this, new EventArgs());
                return;
            }

            var runningGame = runningGameProcesses.First();
            if (
                string.IsNullOrEmpty(this._currentGameProcess) ||
                this._currentGameProcess != runningGame.Key)
            {
                //Switch to new game

                //Stop the previous game data stream
                this._gameDataReader = null;

                //Set new process
                this._currentGameProcess = runningGame.Key;

                this._gameDataReader = Activator.CreateInstance(runningGame.Value) as IGameDataReader;
                if (this._gameDataReader != null)
                {
                    this._gameDataReader.RawDataUpdate += (object? sender, object rawData) =>
                    {
                        if (emitRawData)
                        {
                            this.RawDataUpdated?.Invoke(this, rawData);
                            return;
                        }

                        //Convert to general format
                        var data = this._gameDataReader.Convert(rawData);
                        if (data is RaceData raceData)
                        {
                            this.RaceDataUpdated?.Invoke(this, raceData);
                            return;
                        }

                        if (data is TruckData truckData)
                        {
                            this.TruckDataUpdated?.Invoke(this, truckData);
                            return;
                        }
                    };
                }
            }
        }, cancellationToken, TimeSpan.Zero, TimeSpan.FromSeconds(10));
    }
}
