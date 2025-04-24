using HaddySimHub.Shared;
using System.Diagnostics;
using System.Text.Json;

namespace HaddySimHub
{
    internal static class Updater
    {
        public static async Task<bool> UpdateAvailable()
        {
            // Read version file
            var currentVersion = File.Exists(UpdateConstants.VersionFile) ? File.ReadAllText(UpdateConstants.VersionFile) : null;
            Logger.Info($"Current version: {currentVersion ?? "Unknown"}");

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "HaddySimHub");

            var response = client.GetAsync(UpdateConstants.ReleaseUrl).Result;
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var release = JsonSerializer.Deserialize<Release>(json);

            var latestVersion = release?.TagName;
            var update = currentVersion is null || latestVersion is null || currentVersion != latestVersion;
            if (update)
            {
                Logger.Info($"New version available: {latestVersion}");
                return true;
            }

            Logger.Info($"No update available");
            Logger.Info($"Current version: {(currentVersion is null ? "Unknown" : currentVersion)}");
            return false;
        }

        public static void Update()
        {
            string tempFolder = UpdateConstants.TempFolder;
            if (Directory.Exists(tempFolder))
            {
                Directory.Delete(tempFolder, true);
            }

            Directory.CreateDirectory(tempFolder);

            string updaterFolder = Path.Combine(AppContext.BaseDirectory, "Updater");

            if (!Directory.Exists(updaterFolder))
            {
                Logger.Error("Updater folder not found. Please update manually.");
                return;
            }

            foreach (var file in Directory.GetFiles(updaterFolder, "HaddySimHub*.*"))
            {
                string destFile = Path.Combine(tempFolder, Path.GetFileName(file));
                try
                {
                    File.Copy(file, destFile, true);
                }
                catch (Exception ex)
                {
                    Logger.Error($"Error copying file {file} to {destFile}: {ex.Message}\nPlease update manually.");
                }
            }

            string updaterPath = Path.Combine(tempFolder, "HaddySimHubUpdater.exe");
            if (!File.Exists(updaterPath))
            {
                Logger.Error("Updater not found. Please update manually.");
                return;
            }

            try
            {
                Process.Start(updaterPath, AppContext.BaseDirectory);
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                Logger.Error($"Starting updater failed: {ex.Message}");
            }
        }
    }
}
