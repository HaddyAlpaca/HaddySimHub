using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HaddySimHub
{
    internal class Updater
    {
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool MoveFileEx(string lpExistingFileName, string? lpNewFileName, int dwFlags);
        private const int MOVEFILE_DELAY_UNTIL_REBOOT = 0x00000004;
        private readonly string _releaseUrl = "https://api.github.com/repos/HaddyAlpaca/HaddySimHub/releases/latest";
        private readonly string _versionFile = Path.Combine(AppContext.BaseDirectory, "version.txt");
        private readonly string _tempFolder = Path.Combine(Path.GetTempPath(), $"HaddySimHubUpdater_{Guid.NewGuid()}");

        public async Task<bool> UpdateAvailable()
        {
            // Read version file
            var currentVersion = File.Exists(_versionFile) ? File.ReadAllText(_versionFile) : null;
            Console.WriteLine($"Current version: {currentVersion ?? "Unknown"}");

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "HaddySimHub");

            var response = client.GetAsync(_releaseUrl).Result;
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

        public void Update()
        {
            StartUpdater();
            MarkTempFolderForDeleteOnReboot();
        }

        private void StartUpdater()
        {
            if (!Directory.Exists(_tempFolder))
            {
                Directory.CreateDirectory(_tempFolder);
            }

            foreach (var file in Directory.GetFiles(AppContext.BaseDirectory, "HaddySimHubUpdater.*"))
            {
                string destFile = Path.Combine(_tempFolder, Path.GetFileName(file));
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

            string updaterPath = Path.Combine(_tempFolder, "HaddySimHubUpdater.exe");
            if (!File.Exists(updaterPath))
            {
                Console.WriteLine("Updater not found. Please update manually.");
                return;
            }

            Process.Start(updaterPath, AppContext.BaseDirectory);
        }

        private void MarkTempFolderForDeleteOnReboot()
        {
            // Mark the temporary folder for deletion on the next reboot
            try
            {
                if (!MoveFileEx(_tempFolder, null, MOVEFILE_DELAY_UNTIL_REBOOT))
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }

                Console.WriteLine($"Temp folder marked for deletion on next reboot: {_tempFolder}");
            }
            catch (Win32Exception ex)
            {
                Console.WriteLine($"An error occurred while marking temp folder for deletion: {ex.Message}");
            }
            return;
        }
    }

    internal class Release
    {
        [JsonPropertyName("tag_name")]
        public string TagName { get; set; } = string.Empty;
    }
}
