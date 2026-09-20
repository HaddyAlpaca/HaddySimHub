namespace HaddySimHub.Infrastructure;

public static class UpdateChecker
{
    public static async Task CheckAsync()
    {
        try
        {
            if (await Updater.UpdateAvailable())
            {
                Logger.Info("Update available. Starting updater...");
                Updater.Update();
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"Error checking for updates: {ex.Message}\n\n{ex.StackTrace}");
        }
    }
}
