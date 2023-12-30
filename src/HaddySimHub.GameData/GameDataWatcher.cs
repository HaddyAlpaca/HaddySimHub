using System.Text.Json;
using HaddySimHub.GameData.Models;
using HaddySimHub.Logging;

namespace HaddySimHub.GameData;

/// <summary>
/// Interface for game polling objects.
/// </summary>
public interface IGameDataWatcher
{
    /// <summary>
    /// No game is longer active.
    /// </summary>
    event EventHandler? GameDataIdle;

    /// <summary>
    /// Notification occured.
    /// </summary>
    event EventHandler<string>? Notification;

    /// <summary>
    /// Truck data updated.
    /// </summary>
    event EventHandler<TruckData>? TruckDataUpdated;

    /// <summary>
    /// Race data updated.
    /// </summary>
    event EventHandler<RaceData>? RaceDataUpdated;

    /// <summary>
    /// Start monitoring game data.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    void Start(CancellationToken cancellationToken);
}

public class GameDataWatcher(
    Dictionary<string, Type> readers,
    IProcessMonitor processMonitor,
#pragma warning disable SA1009 // Closing parenthesis should be spaced correctly
    ILogger logger) : IGameDataWatcher
#pragma warning restore SA1009 // Closing parenthesis should be spaced correctly
{
    private readonly Dictionary<string, Type> readers = readers;
    private readonly IProcessMonitor processMonitor = processMonitor;
    private readonly ILogger logger = logger;
    private string currentGameProcess = string.Empty;
    private Timer? processTimer;
    private GameDataReaderBase? gameDataReader;

    public event EventHandler? GameDataIdle;

    public event EventHandler<string>? Notification;

    public event EventHandler<TruckData>? TruckDataUpdated;

    public event EventHandler<RaceData>? RaceDataUpdated;

    public void Start(CancellationToken cancellationToken)
    {
        this.currentGameProcess = string.Empty;

        this.logger.Info("Start watching games");

        // Monitor processes every 10 seconds
        this.processTimer = new Timer(
            _ =>
        {
            // Get the process that is running
            var runningGameProcesses = this.readers.Where(r => this.processMonitor.IsRunning(r.Key));

            if (!runningGameProcesses.Any())
            {
                // No games running
                this.HandleGameStop();
                this.GameDataIdle?.Invoke(this, new EventArgs());
                return;
            }

            var runningGame = runningGameProcesses.First();
            if (
                string.IsNullOrEmpty(this.currentGameProcess) ||
                this.currentGameProcess != runningGame.Key)
            {
                // Switch to new game
                this.logger.Info($"Game activated: {runningGame.Key}");

                // Stop the previous game data stream
                this.HandleGameStop();

                try
                {
                    this.gameDataReader = Activator.CreateInstance(runningGame.Value, new object[] { this.logger }) as GameDataReaderBase;
                    this.gameDataReader!.Initialize();

                    // Set new process
                    this.currentGameProcess = runningGame.Key;
                }
                catch (Exception ex)
                {
                    this.logger.Error($"Error creating game data reader '{runningGame.Value}': {ex.Message}");
                }

                if (this.gameDataReader != null)
                {
                    this.logger.Info($"Subscribe to game events ({this.gameDataReader.GetType().FullName})");
                    this.gameDataReader.RawDataUpdate += this.GameDataReader_RawDataUpdate;
                    this.gameDataReader.Notification += this.GameDataReader_Notification;
                }
            }
        },
            cancellationToken,
            TimeSpan.Zero,
            TimeSpan.FromSeconds(10));
    }

    private void GameDataReader_Notification(object? sender, string message)
    {
        this.logger.Debug($"Send notification: {message}");
        this.Notification?.Invoke(this, message);
    }

    private void GameDataReader_RawDataUpdate(object? sender, object rawData)
    {
        // Convert to general format
        object? data = null;
        try
        {
            data = this.gameDataReader?.Convert(rawData);
        }
        catch (Exception ex)
        {
            this.logger.Error($"Error converting game data: {ex.Message}\n\n{ex.StackTrace}");
        }

        if (data == null)
        {
            // No data
            return;
        }

        if (data is RaceData raceData)
        {
            this.logger.Debug($"Send race data update: {JsonSerializer.Serialize(raceData)}");
            this.RaceDataUpdated?.Invoke(this, raceData);
            return;
        }

        if (data is TruckData truckData)
        {
            this.logger.Debug("Send truck data update: {JsonSerializer.Serialize(truckData)}");
            this.TruckDataUpdated?.Invoke(this, truckData);
            return;
        }
    }

    private void HandleGameStop()
    {
        // Stop the previous game data stream
        if (this.gameDataReader != null)
        {
            this.logger.Info($"Unsubscribe from game events ({this.gameDataReader.GetType().FullName})");

            this.gameDataReader.RawDataUpdate -= this.GameDataReader_RawDataUpdate;
            this.gameDataReader.Notification -= this.GameDataReader_Notification;
            this.gameDataReader = null;
        }
    }
}
