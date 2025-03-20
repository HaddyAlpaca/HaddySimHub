using System.Text.Json;
using System.Text.Json.Serialization;

namespace HaddySimHub
{
    internal class Updater
    {
        private static readonly string _releaseUrl = "https://api.github.com/repos/HaddyAlpaca/HaddySimHub/releases/latest";
        private static readonly string _versionFile = "version.txt";

        public async static Task<bool> UpdateAvailable()
        {
            //Read version file
            var currentVersion = File.Exists(_versionFile) ? File.ReadAllText(_versionFile) : null;

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

            return false;
        }
    }

    internal class Release
    {
        [JsonPropertyName("tag_name")]
        public string TagName { get; set; } = string.Empty;
    }
}
