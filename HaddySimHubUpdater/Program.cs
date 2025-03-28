﻿using System.Net.Http.Json;
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
KillProcess("HaddySimHubUpdater");

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
    KillProcess("HaddySimHub");

    // Extract the ZIP file
    Console.WriteLine($"Extracting {zipFilePath} to {exeFolder}...");
    ZipFile.ExtractToDirectory(zipFilePath, exeFolder, true);
    Console.WriteLine($"Extraction complete. Files are in {exeFolder}");

    //Create version file
    Console.WriteLine($"Creating version file: {release.TagName}");
    File.WriteAllText(Path.Combine(exeFolder, "version.txt"), release.TagName);

    // Start the application
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
            process.WaitForExit(5000); // Wait for 5 seconds
            if (!process.HasExited)
            {
                Console.WriteLine($"Process {process.ProcessName} (PID {process.Id}) did not exit in time...");
            }
        }
    }
}
