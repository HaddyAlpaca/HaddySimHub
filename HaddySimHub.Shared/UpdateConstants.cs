namespace HaddySimHub.Shared;

public static class UpdateConstants
{
    public static readonly string ReleaseUrl = "https://api.github.com/repos/HaddyAlpaca/HaddySimHub/releases/latest";
    public static readonly string VersionFile = Path.Combine(AppContext.BaseDirectory, "version.txt");
    public static readonly string TempFolder = Path.Combine(Path.GetTempPath(), "HaddySimHubUpdater");
    public static readonly string ExePath = Path.Combine(AppContext.BaseDirectory, "HaddySimHub.exe");
    public static readonly string AssetName = "haddy-simhub.zip";

}
