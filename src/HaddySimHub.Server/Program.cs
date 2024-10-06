using HaddySimHub.Server.GameData;
using HaddySimHub.GameData;
using HaddySimHub.WebServer;
using Microsoft.Extensions.Hosting;
using NLog;
using NLog.Config;
using NLog.Targets;

HaddySimHub.Server.Logging.ILogger logger = new HaddySimHub.Server.Logging.Logger("main");
CancellationTokenSource cancellationTokenSource = new();
CancellationToken token = cancellationTokenSource.Token;
IEnumerable<Game> games = [];
Server webServer = new();
DisplayUpdate idleDisplayUpdate = new DisplayUpdate { Type = DisplayType.None };

SetupLogging(args.Contains("--debug"));

var appHost = Host.CreateDefaultBuilder().Build();

var webServerTask = new Task(async () =>
{
    await appHost!.StartAsync();

    // Start the webserver
    webServer.Start(token);
}, token);

if (args.Contains("--simulate"))
{
    logger.Info("Start game simulation...");
    games = [new SimulateGame()];
}
else
{
    // Setup games
    games =
    [
        new Ets2Game(),
        new IRacingGame(),
        new Dirt2Game(),
    ];
}

var processTask = new Task(async () => {
    games.ForEach((game) => {
        game.DisplayUpdate += async (sender, update) => await SendDisplayUpdate(update);
    });

    // Monitor processes
    IEnumerable<Game> currentGames = [];
    while (!token.IsCancellationRequested)
    {
        var runningGames = games.Where(g => g.IsRunning).ToList();
        if (runningGames.Count == 0)
        {
            await SendDisplayUpdate(idleDisplayUpdate);
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
            FileName = "log/${date:format=yyyy-MM-dd}-${logger}-data.log",
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
        FileName = "log/${date:format=yyyy-MM-dd}.log",
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

async Task SendDisplayUpdate(DisplayUpdate update)
{
    logger.LogData(update);
    await GameDataHub.SendDisplayUpdate(update);
}