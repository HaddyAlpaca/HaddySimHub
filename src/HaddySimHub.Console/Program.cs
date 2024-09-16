﻿using System.Diagnostics;
using System.IO.Compression;
using System.Text.RegularExpressions;
using HaddySimHub.DirtRally2;
using HaddySimHub.GameData;
using HaddySimHub.WebServer;
using Microsoft.Extensions.Hosting;
using NLog;
using NLog.Config;
using NLog.Targets;

HaddySimHub.Logging.ILogger logger = new HaddySimHub.Logging.Logger("main");
CancellationTokenSource cancellationTokenSource = new();
CancellationToken token = cancellationTokenSource.Token;
IEnumerable<Game> games = [];
Server webServer = new();

SetupLogging(args.Contains("--debug"));

//Check for website update
await UpdateWebContent(token);

var appHost = Host.CreateDefaultBuilder().Build();

var webServerTask = new Task(async () =>
{
    await appHost!.StartAsync();

    // Start the webserver
    webServer.Start(token);
}, token);
        
var processTask = new Task(async () => {
    // Setup games
    games =
    [
        new Ets2Game(),
        new IRacingGame(),
        new Dirt2Game(),
    ];

    games.ForEach((game) => {
        game.DisplayUpdate += OnGameDisplayUpdate;
    });

    // Monitor processes
    IEnumerable<Game> currentGames = [];
    while (!token.IsCancellationRequested)
    {
        var runningGames = games.Where(g => IsProcessRunning(g.ProcessName)).ToList();
        if (runningGames.Count == 0)
        {
            var update = new DisplayUpdate { Type = DisplayType.None };
            logger.LogData(update);
            await NotificationService.SendDisplayUpdate(update);
        }

        runningGames.Where(g => !currentGames.Any(r => r.Description == g.Description)).ForEach(g => {
            try
            {
                g.Start();
            }
            catch (Exception ex)
            {
                logger.Error($"Error starting datafeed of game {g.Description}: {ex.Message}\n\n{ex.StackTrace}");
            }
        });
        currentGames.Where(g => !runningGames.Any(c => c.Description == g.Description)).ForEach(g => {
            try
            {
                g.Stop();
            }
            catch (Exception ex)
            {
                logger.Error($"Error stoping datafeed of game {g.Description}: {ex.Message}\n\n{ex.StackTrace}");
            }
        });
        currentGames = runningGames;
        await Task.Delay(TimeSpan.FromSeconds(2));
    }
}, token);

try
{
    webServerTask.Start();
    processTask.Start();
}
catch(Exception ex)
{
    logger.Fatal($"Unhandled Exception: {ex.Message}\n\n{ex.StackTrace}");
}

appHost.WaitForShutdown();
cancellationTokenSource.Cancel();
Task.WaitAll([webServerTask, processTask]);

void SetupLogging(bool debug)
{
    var logConfig = new LoggingConfiguration();

    var consoleTarget = new ColoredConsoleTarget
    {
        Layout = @"${message}"
    };
    logConfig.LoggingRules.Add(new LoggingRule(
        "*",
        LogLevel.Debug,
        LogLevel.Fatal,
        consoleTarget));
    logConfig.AddTarget("console", consoleTarget);

    if (debug)
    {
        // Setup data logging
        var debugTarget = new FileTarget
        {
            FileName = "log\\${date:format=yyyy-MM-dd}-${logger}-data.log",
            Layout = @"${message}",
        };

        logConfig.LoggingRules.Add(new LoggingRule(
            "*",
            LogLevel.Trace,
            LogLevel.Trace,
            debugTarget));
        logConfig.AddTarget("data-logfile", debugTarget);
    }

    // General
    var fileTarget = new FileTarget
    {
        FileName = "log\\${date:format=yyyy-MM-dd}.log",
        Layout = @"${longdate} ${uppercase:${level}}: ${message}",
    };

    logConfig.LoggingRules.Add(new LoggingRule(
        "*",
        debug ? LogLevel.Debug : LogLevel.Info,
        LogLevel.Fatal,
        fileTarget));
    logConfig.AddTarget("general-logfile", fileTarget);

    LogManager.Configuration = logConfig;
}

async Task UpdateWebContent(CancellationToken token)
{
    string rootfolder = Path.GetFullPath("wwwroot");
    logger.Debug($"Check if folder '{rootfolder}' exists.");
    if (!Directory.Exists(rootfolder))
    {
        try
        {
            logger.Debug($"Create folder '{rootfolder}'.");
            Directory.CreateDirectory(rootfolder);
        }
        catch (Exception ex)
        {
            logger.Error($"Client content folder cannot be created: {ex.Message}");
        }
    }

    if (Directory.Exists(rootfolder))
    {
        // Check if new content needs to be downloaded
        string zipFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.zip");

        try
        {
            const string baseUri = "https://github.com/HaddyAlpaca/HaddySimHubClient";

            logger.Info($"Checking latest webcontent on '{baseUri}'.");
            string versionUrl = $"{baseUri}/releases/latest";
            using HttpClient client = new();
            string content = await client.GetStringAsync(versionUrl, token);

            Match match = Regex.Match(content, @"Release v(\S+)");

            if (match.Success)
            {
                string version = match.Groups[1].Value; // Extract text captured by the first group
                string downloadUrl = $"{baseUri}/releases/download/v{version}/haddy-simhub-client.zip";
                logger.Info($"Downloading latest web content version from '{downloadUrl}'");
                HttpResponseMessage response = await client.GetAsync(downloadUrl, token);

                using Stream fileStream = await response.Content.ReadAsStreamAsync(token);
                using FileStream outputFileStream = File.Create(zipFile);
                await fileStream.CopyToAsync(outputFileStream, token);
                outputFileStream.Close();

                //Delete current files in directory
                var dir = new DirectoryInfo(rootfolder);
                dir.Delete(true);

                // Extract the downloaded ZIP file to the specified folder
                logger.Info($"Extract file '{zipFile}' to '{rootfolder}'");
                ZipFile.ExtractToDirectory(zipFile, rootfolder, true);
            }
        }
        catch (Exception ex)
        {
            logger.Error($"Error checking for client update:\n\n{ex.Message}");
        }
        finally
        {
            if (File.Exists(zipFile))
            {
                try
                {
                    logger.Info($"Deleting file '{zipFile}'.");
                    File.Delete(zipFile); // Delete the temporary ZIP file
                }
                catch (Exception ex)
                {
                    logger.Error($"Error deleting file:\n\n{ex.Message}");
                }
            }
        }
    }
}

async void OnGameDisplayUpdate(object? sender, DisplayUpdate update)
{
    logger.LogData(update);
    await NotificationService.SendDisplayUpdate(update);
}

bool IsProcessRunning(string processName)
{
    return Process.GetProcessesByName(processName).Length != 0;
}