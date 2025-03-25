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
            Console.WriteLine($"Current version: {currentVersion ?? "Unknown"}");

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
                Console.WriteLine($"New version available: {latestVersion}");
                return true;
            }

            Console.WriteLine($"No update available");
            Console.WriteLine($"Current version: {(currentVersion is null ? "Unknown" : currentVersion)}");
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

            foreach (var file in Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "Updater"), "HaddySimHub*.*"))
            {
                string destFile = Path.Combine(tempFolder, Path.GetFileName(file));
                try
                {
                    File.Copy(file, destFile, true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error copying file {file} to {destFile}: {ex.Message}");
                    Console.WriteLine("Please update manually.");
                }
            }

            string updaterPath = Path.Combine(tempFolder, "HaddySimHubUpdater.exe");
            if (!File.Exists(updaterPath))
            {
                Console.WriteLine("Updater not found. Please update manually.");
                return;
            }

            Process.Start(updaterPath, AppContext.BaseDirectory);
        }
    }
}
