using HaddySimHub.GameData.Models;
using HaddySimHub.Logging;

namespace HaddySimHub.GameData;

public interface IGameDataWatcher
{
    event EventHandler? GameDataIdle;
    event EventHandler<object>? RawDataUpdated;
    event EventHandler<TruckData>? TruckDataUpdated;
    event EventHandler<RaceData>? RaceDataUpdated;
    void Start(GameDataWatcherOptions options, CancellationToken cancellationToken);
}

public class GameDataWatcherOptions
{
    public bool EmitRawData { get; init; }
    public bool RunDemoMode { get; init; }
}

public class GameDataWatcher(
    Dictionary<string, Type> readers,
    IProcessMonitor processMonitor,
    ILogger logger) : IGameDataWatcher
{
    private readonly Dictionary<string, Type> readers = readers;
    private readonly IProcessMonitor processMonitor = processMonitor;
    private readonly ILogger _logger = logger;
    private string _currentGameProcess = string.Empty;
    private Timer? _processTimer;
    private IGameDataReader? _gameDataReader;

    public event EventHandler? GameDataIdle;
    public event EventHandler<object>? RawDataUpdated;
    public event EventHandler<TruckData>? TruckDataUpdated;
    public event EventHandler<RaceData>? RaceDataUpdated;

    public void Start(GameDataWatcherOptions options, CancellationToken cancellationToken)
    {
        this._currentGameProcess = string.Empty;

        this._logger.Info("Start watching games");

        if (options.RunDemoMode)
        {
            this._logger.Info("Running demo mode");

            Task.Run(() =>
            {
                Thread.Sleep(10000);

                //Truck data
                for (short i = 1; i <= 12; i++)
                {
                    this.TruckDataUpdated?.Invoke(this, new TruckData { Gear = i });

                    Thread.Sleep(1000);
                }

                //Race data
                for (short i = 1; i <= 12; i++)
                {
                    this.RaceDataUpdated?.Invoke(this, new RaceData { Gear = i });

                    Thread.Sleep(1000);
                }

                //Idle
                this.GameDataIdle?.Invoke(this, new EventArgs());
            });

        }
        else
        {
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
                    this._logger.Info($"Game activated: {runningGame.Key}");

                    //Stop the previous game data stream
                    this._gameDataReader = null;

                    try
                    {
                        this._gameDataReader = Activator.CreateInstance(runningGame.Value) as IGameDataReader;

                        //Set new process
                        this._currentGameProcess = runningGame.Key;
                    }
                    catch (Exception ex)
                    {
                        this._logger.Error($"Error creating game data reader '{runningGame.Value}': {ex.Message}");
                    }

                    if (this._gameDataReader != null)
                    {
                        this._gameDataReader.RawDataUpdate += (object? sender, object rawData) =>
                        {
                            if (options.EmitRawData)
                            {
                                this.RawDataUpdated?.Invoke(this, rawData);
                                return;
                            }

                            //Convert to general format
                            object? data = null;
                            try
                            {
                                data = this._gameDataReader.Convert(rawData);
                            }
                            catch (Exception ex)
                            {
                                this._logger.Error($"Error converting game data: {ex.Message}");
                            }

                            if (data == null)
                            {
                                //No data
                                return;
                            }

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
}
