using System.Net.Http.Json;
using System.IO.Compression;
using System.Diagnostics;
using HaddySimHub.Shared;

if (args.Length == 0)
{
    Console.WriteLine("Usage: HaddySimHubUpdater <folder_of_executable>");
    return;
}

string exeFolder = args[0];

// Ensure single instance of the application
Mutex mutex = new(true, "HaddySimHubUpdater_SingleInstance", out bool createdNew);
if (!createdNew)
{
    Console.WriteLine("Another instance of HaddySimHubUpdater is already running.");
    return;
}

string zipFilePath = string.Empty;
try
{
    HttpClient client = new();

    // Set the User-Agent header as required by GitHub API
    client.DefaultRequestHeaders.Add("User-Agent", "HaddySimHub Updater");

    // Fetch the latest release information
    var release = await client.GetFromJsonAsync<Release>(UpdateConstants.ReleaseUrl);
    if (release == null)
    {
        Console.WriteLine("Failed to fetch release information.");
        return;
    }

    // Extract the assets array
    var downloadUrl = release.Assets.FirstOrDefault(a => a.Name.Equals(UpdateConstants.AssetName))?.BrowserDownloadUrl;
    if (string.IsNullOrEmpty(downloadUrl))
    {
        Console.WriteLine("No assets found in the latest release.");
        return;
    }

    // Define the paths
    var tempFolderPath = Path.GetTempPath();
    zipFilePath = Path.Combine(tempFolderPath, UpdateConstants.AssetName);

    // Download the asset
    Console.WriteLine($"Downloading {UpdateConstants.AssetName} to {zipFilePath}...");
    var assetData = await client.GetByteArrayAsync(downloadUrl);
    await File.WriteAllBytesAsync(zipFilePath, assetData);
    Console.WriteLine($"Downloaded and saved to {zipFilePath}");

    // Ensure the destination folder exists
    if (!Directory.Exists(exeFolder))
    {
        Directory.CreateDirectory(exeFolder);
    }

    //Stop the application
    Process[] processes = Process.GetProcessesByName("HaddySimHub");
    foreach (Process process in processes)
    {
        Console.WriteLine($"Stopping {process.ProcessName} (PID {process.Id})...");
        process.Kill();
        //Wait for the process to exit
        process.WaitForExit(5000); // Wait for 5 seconds
        if (!process.HasExited)
        {
            Console.WriteLine($"Process {process.ProcessName} (PID {process.Id}) did not exit in time...");
        }
    }

    // Extract the ZIP file
    Console.WriteLine($"Extracting {zipFilePath} to {exeFolder}...");
    ZipFile.ExtractToDirectory(zipFilePath, exeFolder, true);
    Console.WriteLine($"Extraction complete. Files are in {exeFolder}");

    //Create version file
    Console.WriteLine($"Creating version file: {release.TagName}");
    File.WriteAllText(Path.Combine(exeFolder, "version.txt"), release.TagName);

    // Start the application
    if (File.Exists(UpdateConstants.ExePath))
    {
        Console.WriteLine($"Starting {UpdateConstants.ExePath}...");

        try
        {
            Process.Start(UpdateConstants.ExePath);
        }
        catch (Exception startEx)
        {
            Console.WriteLine($"Failed to start {UpdateConstants.ExePath}: {startEx.Message}");
        }
    }
    else
    {
        Console.WriteLine($"Executable not found: {UpdateConstants.ExePath}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred: {ex.Message}");
}
finally
{
    // Delete the ZIP file after extraction and execution attempt
    if (!string.IsNullOrEmpty(zipFilePath) && File.Exists(zipFilePath))
    {
        try
        {
            File.Delete(zipFilePath);
            Console.WriteLine($"Deleted temp file: {zipFilePath}");
        }
        catch (Exception deleteEx)
        {
            Console.WriteLine($"Failed to delete ZIP file: {deleteEx.Message}");
        }
    }
}
