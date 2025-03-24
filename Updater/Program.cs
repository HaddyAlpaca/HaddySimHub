using System.Net.Http.Json;
using System.IO.Compression;
using System.Diagnostics;
using Updater;

// Ensure single instance of the application
Mutex mutex = new(true, "HaddySimHubUpdater_SingleInstance", out bool createdNew);
if (!createdNew)
{
    Console.WriteLine("Another instance of the application is already running.");
    return;
}

HttpClient client = new();
string apiUrl = "https://api.github.com/repos/HaddyAlpaca/HaddySimHub/releases/latest";
string exePath = @"C:\HaddySimHub\HaddySimHub.exe";
string assetName = "haddy-simhub.zip";

string zipFilePath = string.Empty;

try
{
    // Set the User-Agent header as required by GitHub API
    client.DefaultRequestHeaders.Add("User-Agent", "HaddySimHub Updater");

    // Fetch the latest release information
    var release = await client.GetFromJsonAsync<Release>(apiUrl);
    if (release == null)
    {
        Console.WriteLine("Failed to fetch release information.");
        Console.ReadLine(); // Prevent immediate exit
        return;
    }

    // Extract the assets array
    var downloadUrl = release.Assets.FirstOrDefault(a => a.Name.Equals("haddy-simhub.zip"))?.BrowserDownloadUrl;
    if (string.IsNullOrEmpty(downloadUrl))
    {
        Console.WriteLine("No assets found in the latest release.");
        Console.ReadLine(); // Prevent immediate exit
        return;
    }

    // Define the paths
    var tempFolderPath = Path.GetTempPath();
    zipFilePath = Path.Combine(tempFolderPath, assetName);
    var extractPath = Path.GetDirectoryName(exePath);

    // Download the asset
    Console.WriteLine($"Downloading {assetName} to {zipFilePath}...");
    var assetData = await client.GetByteArrayAsync(downloadUrl);
    await File.WriteAllBytesAsync(zipFilePath, assetData);
    Console.WriteLine($"Downloaded and saved to {zipFilePath}");

    // Ensure the destination folder exists
    if (!Directory.Exists(extractPath))
    {
        Directory.CreateDirectory(extractPath!);
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

    //Delete the old files and folders
    Console.WriteLine("Deleting old version...");
    if (Directory.Exists(extractPath))
    {
        Directory.Delete(extractPath, true);
    }

    // Extract the ZIP file
    Console.WriteLine($"Extracting {zipFilePath} to {extractPath}...");
    ZipFile.ExtractToDirectory(zipFilePath, extractPath!, true);
    Console.WriteLine($"Extraction complete. Files are in {extractPath}");

    //Create version file
    Console.WriteLine($"Creating version file: {release.TagName}");
    File.WriteAllText(Path.Combine(extractPath!, "version.txt"), release.TagName);

    // Start the application
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
            // Log the exception for further investigation
        }
    }
    else
    {
        Console.WriteLine($"Executable not found: {exePath}");
        Console.ReadLine(); // Prevent immediate exit if the exe is missing
    }
}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred: {ex.Message}");
    Console.ReadLine(); // Prevent immediate exit on error
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
            Console.ReadLine(); // Prevent immediate exit if the file can't be deleted
        }
    }
}
