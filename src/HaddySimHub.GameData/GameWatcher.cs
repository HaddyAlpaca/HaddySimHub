namespace HaddySimHub.GameData;

public class GameWatcher
{
    public GameWatcher(IEnumerable<Game> games)
    {
        foreach (var game in games)
        {
            game.DisplayUpdate += (s, e) => this.DisplayUpdate?.Invoke(this, e);
            game.Notification += (s, e) => this.Notification?.Invoke(this, e);
            game.Stopped += (sender, e) =>
            {
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
