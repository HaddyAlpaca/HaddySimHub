namespace HaddySimHub.GameData;

public class GameWatcher
{
    public GameWatcher(IEnumerable<Game> games)
    {
        foreach (var game in games)
        {
            game.Started += (sender, e) =>
            {
                if (sender is Game game)
                {
                    this.Notification?.Invoke(this, $"Game activated: {game.Description}");
                }
            };

            game.Stopped += (sender, e) =>
            {
                if (sender is Game game)
                {
                    this.Notification?.Invoke(this, $"Game stopped: {game.Description}");
                }

                if (!games.Any(g => g.IsRunning))
                {
                    // No games running
                    this.DisplayUpdate?.Invoke(this, new DisplayUpdate { Type = DisplayType.None });
                }
            };
        }
    }

    public event EventHandler<string>? Notification;

    public event EventHandler<DisplayUpdate>? DisplayUpdate;
}
