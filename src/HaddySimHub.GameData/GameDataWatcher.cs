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
    IEnumerable<Game> games,
    IProcessMonitor processMonitor,
#pragma warning disable SA1009 // Closing parenthesis should be spaced correctly
    ILogger logger) : IGameDataWatcher
#pragma warning restore SA1009 // Closing parenthesis should be spaced correctly
{
    private readonly IEnumerable<Game> games = games;
    private readonly IProcessMonitor processMonitor = processMonitor;
    private readonly ILogger logger = logger;
    private Game? currentGame;
    private Timer? processTimer;
    private GameDataReaderBase? gameDataReader;

    public event EventHandler<DisplayUpdate>? DisplayDataUpdated;

    public event EventHandler<string>? Notification;

    public void Start(CancellationToken cancellationToken)
    {
        this.logger.Info("Start watching games");

        // Monitor processes every 10 seconds
        this.processTimer = new Timer(
            _ =>
        {
            // Get the process that is running
            var currentGame = this.games.FirstOrDefault(g => this.processMonitor.IsRunning(g.ProcessName));

            if (currentGame is null || currentGame.ProcessName != this.currentGame?.ProcessName)
            {
                // No games running or another game is running

                // Set the new active game
                this.currentGame = currentGame;

                // Stop the previous game reader
                this.gameDataReader = null;

                if (currentGame is null)
                {
                    // No games active
                    this.DisplayDataUpdated?.Invoke(this, new DisplayUpdate { Type = DisplayType.None });
                    return;
                }
                else
                {
                    // Switch to new game
                    this.Notification?.Invoke(this, $"Game activated: {currentGame.Description}");

                    try
                    {
                        this.gameDataReader = Activator.CreateInstance(currentGame.Type) as GameDataReaderBase;

                        if (this.gameDataReader != null)
                        {
                            this.gameDataReader.RawDataUpdate += this.GameDataReader_RawDataUpdate;
                            this.gameDataReader.Notification += (s, message) =>
                                this.Notification?.Invoke(this, message);
                        }
                    }
                    catch (Exception ex)
                    {
                        this.logger.Error($"Error creating game data reader '{currentGame.Type}': {ex.Message}");
                    }
                }
            }
        },
            cancellationToken,
            TimeSpan.Zero,
            TimeSpan.FromSeconds(10));
    }

    private void GameDataReader_RawDataUpdate(object? sender, object rawData)
    {
        this.Notification?.Invoke(this, $"{DateTime.Now} RawDataUpdate");
        this.logger.LogData(rawData);

        // Convert to general format
        try
        {
            var data = this.gameDataReader!.Convert(rawData);
            if (data is not null)
            {
                this.DisplayDataUpdated?.Invoke(this, new DisplayUpdate
                {
                    Type = this.gameDataReader.CurrentDisplayType,
                    Data = data,
                });
            }
        }
        catch (Exception ex)
        {
            this.logger.Error($"Error converting game data: {ex.Message}\n\n{ex.StackTrace}");
            return;
        }
    }
}
