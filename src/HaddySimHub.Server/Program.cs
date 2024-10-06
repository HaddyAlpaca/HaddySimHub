using HaddySimHub.Server.Models;
using HaddySimHub.Server.Games;
using Microsoft.Extensions.Hosting;
using NLog;
using NLog.Config;
using NLog.Targets;
using HaddySimHub.Server;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

HaddySimHub.Server.Logging.ILogger logger = new HaddySimHub.Server.Logging.Logger("main");
CancellationTokenSource cancellationTokenSource = new();
CancellationToken token = cancellationTokenSource.Token;
IEnumerable<Game> games = [];
DisplayUpdate idleDisplayUpdate = new() { Type = DisplayType.None };

SetupLogging(args.Contains("--debug"));
WebApplicationOptions options = new()
{
    ContentRootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
};

if (options.ContentRootPath is not null)
{
    string wwwRoot = Path.Combine(options.ContentRootPath, "wwwroot");
    if (!Directory.Exists(wwwRoot))
    {
        Directory.CreateDirectory(wwwRoot);
    }
}

var builder = WebApplication.CreateBuilder(options);
builder.WebHost.UseKestrel(options =>
{
    options.ListenAnyIP(3333);
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod()
            .SetIsOriginAllowed(origin => true);
    });
});
builder.Services.AddControllers();
builder.Services.AddSignalR(o =>
{
    o.EnableDetailedErrors = true;
});

var webServer = builder.Build();
webServer.UseRouting();
webServer.UseDefaultFiles();
webServer.UseStaticFiles();
webServer.UseCors();
webServer.MapHub<GameDataHub>("/display-data");

var webServerTask = new Task(async () =>
{
    await webServer!.StartAsync(token);
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
        new HaddySimHub.Server.Games.Ets2.Ets2Game(),
        new HaddySimHub.Server.Games.iRacing.IRacingGame(),
        new HaddySimHub.Server.Games.DirtRally2.Dirt2Game(),
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

webServer.WaitForShutdown();
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