using System.Diagnostics;
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

async void OnGameDisplayUpdate(object? sender, DisplayUpdate update)
{
    logger.LogData(update);
    await NotificationService.SendDisplayUpdate(update);
}

bool IsProcessRunning(string processName)
{
    return Process.GetProcessesByName(processName).Length != 0;
}