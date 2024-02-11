using HaddySimHub.Logging;

namespace HaddySimHub.GameData;

public enum DisplayType
{
    None,
    TruckDashboard,
    RaceDashboard,
}

public sealed record DisplayUpdate
{
    public DisplayType Type { get; init; }

    public object? Data { get; init; }
}

/// <summary>
/// Interface for game polling objects.
/// </summary>
public interface IGameDataWatcher
{
    /// <summary>
    /// Display update.
    /// </summary>
    event EventHandler<DisplayUpdate>? DisplayDataUpdated;

    /// <summary>
    /// Notification occured.
    /// </summary>
    event EventHandler<string>? Notification;

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

    public event EventHandler<DisplayUpdate>? DisplayDataUpdated;

    public event EventHandler<string>? Notification;

    public void Start(CancellationToken cancellationToken)
    {
        this.currentGameProcess = string.Empty;

        this.logger.Info("Start watching games");

        // Monitor processes every 10 seconds
        this.processTimer = new Timer(
            _ =>
        {
            this.Notification?.Invoke(this, $"Alive {DateTime.Now}");

            // Get the process that is running
            var runningGameProcesses = this.readers.Where(r => this.processMonitor.IsRunning(r.Key));

            if (!runningGameProcesses.Any())
            {
                // No games running
                this.HandleGameStop();
                this.DisplayDataUpdated?.Invoke(this, new DisplayUpdate { Type = DisplayType.None });
                return;
            }

            var runningGame = runningGameProcesses.First();
            if (
                string.IsNullOrEmpty(this.currentGameProcess) ||
                this.currentGameProcess != runningGame.Key)
            {
                // Switch to new game
                this.Notify($"Game activated: {runningGame.Key}");

                // Stop the previous game data stream
                this.HandleGameStop();

                try
                {
                    this.gameDataReader = Activator.CreateInstance(runningGame.Value, new object[] { this.logger }) as GameDataReaderBase;

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
        this.logger.Info($"Send notification: {message}");
        this.Notify(message);
    }

    private void GameDataReader_RawDataUpdate(object? sender, object rawData)
    {
        this.logger.LogData(rawData);

        // Convert to general format
        object? data;
        try
        {
            data = this.gameDataReader!.Convert(rawData);

            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }
        }
        catch (Exception ex)
        {
            this.Notification?.Invoke(this, $"Error converting game data: {ex.Message}\n\n{ex.StackTrace}");
            this.logger.Error($"Error converting game data: {ex.Message}\n\n{ex.StackTrace}");
            return;
        }

        this.logger.Debug("Send display data update.");
        var update = new DisplayUpdate
        {
            Type = this.gameDataReader.CurrentDisplayType,
            Data = data,
        };
        this.DisplayDataUpdated?.Invoke(this, update);
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

    private void Notify(string message)
    {
        this.logger.Debug($"Send notification: {message}");
        this.Notification?.Invoke(this, message);
    }
}
