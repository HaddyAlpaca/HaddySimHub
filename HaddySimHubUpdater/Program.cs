using System.IO.Compression;
using System.Diagnostics;
using System.Text.Json;
using HaddySimHub.Shared;

if (args.Length == 0)
{
    Console.WriteLine("Usage: HaddySimHubUpdater <folder_of_executable>");
    return;
}

string exeFolder = args[0];

KillProcess("HaddySimHubUpdater");

string zipFilePath = string.Empty;
try
{
    HttpClient client = new();
    client.DefaultRequestHeaders.Add("User-Agent", "HaddySimHub Updater");

    var json = await client.GetStringAsync(UpdateConstants.ReleaseUrl);
    var release = JsonSerializer.Deserialize(json, ReleaseContext.Default.Release);
    if (release == null)
    {
        Console.WriteLine("Failed to fetch release information.");
        return;
    }

    var downloadUrl = release.Assets.FirstOrDefault(a => a.Name.Equals(UpdateConstants.AssetName))?.BrowserDownloadUrl;
    if (string.IsNullOrEmpty(downloadUrl))
    {
        Console.WriteLine("No assets found in the latest release.");
        return;
    }

    var tempFolderPath = Path.GetTempPath();
    zipFilePath = Path.Combine(tempFolderPath, UpdateConstants.AssetName);

    Console.WriteLine($"Downloading {UpdateConstants.AssetName}...");
    var assetData = await client.GetByteArrayAsync(downloadUrl);
    await File.WriteAllBytesAsync(zipFilePath, assetData);
    Console.WriteLine("Download complete.");

    if (!Directory.Exists(exeFolder))
        Directory.CreateDirectory(exeFolder);

    KillProcess("HaddySimHub");

    await EnsureRuntime(zipFilePath);

    Console.WriteLine($"Extracting to {exeFolder}...");
    ZipFile.ExtractToDirectory(zipFilePath, exeFolder, true);
    Console.WriteLine("Extraction complete.");

    Console.WriteLine($"Creating version file: {release.TagName}");
    File.WriteAllText(Path.Combine(exeFolder, "version.txt"), release.TagName);

    string exePath = Path.Combine(exeFolder, "HaddySimHub.exe");
    if (File.Exists(exePath))
    {
        Console.WriteLine($"Starting {exePath}...");
        try
        {
            Process.Start(exePath);
        }
        catch (Exception startEx)
        {
            Console.WriteLine($"Failed to start {exePath}: {startEx.Message}");
        }
    }
    else
    {
        Console.WriteLine($"Executable not found: {exePath}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred: {ex.Message}");
}
finally
{
    if (!string.IsNullOrEmpty(zipFilePath) && File.Exists(zipFilePath))
    {
        try
        {
            File.Delete(zipFilePath);
            Console.WriteLine("Deleted temp file.");
        }
        catch (Exception deleteEx)
        {
            Console.WriteLine($"Failed to delete ZIP file: {deleteEx.Message}");
        }
    }
}

static void KillProcess(string name)
{
    Process currentProcess = Process.GetCurrentProcess();
    Process[] processes = Process.GetProcessesByName(name);

    foreach (var process in processes)
    {
        if (process.Id != currentProcess.Id)
        {
            Console.WriteLine($"Killing process {process.ProcessName} with ID {process.Id}.");
            process.Kill();
            process.WaitForExit(5000);
            if (!process.HasExited)
                Console.WriteLine($"Process {process.ProcessName} (PID {process.Id}) did not exit in time.");
        }
    }
}

static async Task EnsureRuntime(string zipPath)
{
    var version = ReadRequiredRuntimeVersion(zipPath);
    if (version == null)
    {
        Console.WriteLine("Could not determine required .NET runtime version (no runtimeconfig.json in release).");
        return;
    }

    Console.WriteLine($"Required .NET runtime: Microsoft.NETCore.App {version}");

    if (IsRuntimeInstalled(version))
    {
        Console.WriteLine("Runtime already installed.");
        return;
    }

    Console.WriteLine("Runtime not found. Installing...");
    await DownloadAndInstallRuntime(version);
}

static string? ReadRequiredRuntimeVersion(string zipPath)
{
    try
    {
        using var archive = ZipFile.OpenRead(zipPath);
        var entry = archive.GetEntry("HaddySimHub.runtimeconfig.json");
        if (entry == null) return null;

        using var reader = new StreamReader(entry.Open());
        using var doc = JsonDocument.Parse(reader.ReadToEnd());
        return doc.RootElement
            .GetProperty("runtimeOptions")
            .GetProperty("framework")
            .GetProperty("version")
            .GetString();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Warning: could not read runtime config: {ex.Message}");
        return null;
    }
}

static bool IsRuntimeInstalled(string version)
{
    try
    {
        var psi = new ProcessStartInfo("dotnet", "--list-runtimes")
        {
            RedirectStandardOutput = true,
            CreateNoWindow = true
        };
        using var process = Process.Start(psi);
        if (process == null) return false;
        var output = process.StandardOutput.ReadToEnd();
        process.WaitForExit(3000);
        return output.Contains($"Microsoft.NETCore.App {version}");
    }
    catch
    {
        return false;
    }
}

static async Task DownloadAndInstallRuntime(string version)
{
    var parts = version.Split('.');
    var channel = $"{parts[0]}.{parts[1]}";

    using var client = new HttpClient();
    var scriptUrl = "https://dot.net/v1/dotnet-install.ps1";
    var scriptPath = Path.Combine(Path.GetTempPath(), "dotnet-install.ps1");

    Console.WriteLine("Downloading dotnet-install.ps1...");
    var script = await client.GetByteArrayAsync(scriptUrl);
    await File.WriteAllBytesAsync(scriptPath, script);

    Console.WriteLine($"Installing .NET runtime {channel} (this may take a while)...");

    var psi = new ProcessStartInfo("powershell")
    {
        Arguments = $"-NoProfile -ExecutionPolicy Bypass -File \"{scriptPath}\" -Channel {channel} -Runtime dotnet",
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        CreateNoWindow = true
    };
    using var process = Process.Start(psi);
    if (process == null)
    {
        Console.WriteLine("Failed to start powershell for runtime installation.");
        return;
    }
    process.OutputDataReceived += (_, e) => { if (e.Data != null) Console.WriteLine($"  {e.Data}"); };
    process.BeginOutputReadLine();
    process.ErrorDataReceived += (_, e) => { if (e.Data != null) Console.WriteLine($"  {e.Data}"); };
    process.BeginErrorReadLine();
    process.WaitForExit();

    try { File.Delete(scriptPath); } catch { }

    if (process.ExitCode != 0)
        Console.WriteLine($"Warning: dotnet-install exited with code {process.ExitCode}.");
    else
        Console.WriteLine(".NET runtime installation complete.");
}
