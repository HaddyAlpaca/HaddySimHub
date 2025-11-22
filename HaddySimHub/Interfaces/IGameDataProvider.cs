namespace HaddySimHub.Interfaces;

public interface IGameDataProvider<T>
{
    /// <summary>
    /// Event that fires when new game data is available.
    /// </summary>
    event EventHandler<T> DataReceived;

    /// <summary>
    /// Starts the data acquisition from the game.
    /// </summary>
    void Start();

    /// <summary>
    /// Stops the data acquisition from the game.
    /// </summary>
    void Stop();
}
