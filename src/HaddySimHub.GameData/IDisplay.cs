using HaddySimHub.GameData;

/// <summary>
/// Display interface.
/// </summary>
public interface IDisplay
{
    /// <summary>
    /// Converts the game data to display data.
    /// </summary>
    /// <param name="inputData">Game data.</param>
    /// <returns>DisplayUpdate object.</returns>
    DisplayUpdate GetDisplayUpdate(object inputData);
}