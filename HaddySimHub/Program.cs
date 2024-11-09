using HaddySimHub.Models;
using Microsoft.Extensions.Hosting;
using NLog;
using NLog.Config;
using NLog.Targets;
using HaddySimHub;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using HaddySimHub.Displays;
using System.Text;

HaddySimHub.Logging.ILogger logger = new HaddySimHub.Logging.Logger("main");
CancellationTokenSource cancellationTokenSource = new();
CancellationToken token = cancellationTokenSource.Token;
IEnumerable<IDisplay> displays = [];
DisplayUpdate idleDisplayUpdate = new() { Type = DisplayType.None };
JsonSerializerOptions serializeOptions = new() { IncludeFields = true };

SetupLogging(args.Contains("--debug"));

WebApplicationOptions options = new()
{
    ContentRootPath = AppContext.BaseDirectory
};

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

// Setup display
displays =
[
    new Dirt2DashboardDisplay(SendDisplayUpdate),
    new IRacingDashboardDisplay(SendDisplayUpdate),
    new Ets2DashboardDisplay(SendDisplayUpdate),
];

var testRun = args.Contains("--test-run");
var processTask = new Task(async () => {
    // Monitor processes
    IEnumerable<IDisplay> prevActiveDisplays = [];
    while (!token.IsCancellationRequested)
    {
        if (testRun)
        {
            var update = new DisplayUpdate
            {
                Type = DisplayType.TruckDashboard,
                Data = new TruckData
                {
                    Speed = (short)DateTime.Now.Second,
                }
            };
            await SendDisplayUpdate(update);
        }
        else
        {
            var activeDisplays = displays.Where(d => d.IsActive).ToList();
            if (activeDisplays.Count == 0)
            {
                logger.Debug("No active displays found");
                await SendDisplayUpdate(idleDisplayUpdate);
            }
            else
            {
                StringBuilder sb = new();
                sb.AppendLine("Active displays:");
                foreach(var display in displays)
                {
                    sb.AppendLine($"{display.Description}");
                }

                logger.Debug(sb.ToString());
            }

            activeDisplays.Where(g => !activeDisplays.Any(x => x.Description == g.Description)).ForEach(d => {
                try
                {
                    logger.Info($"Start receiving data from {d.Description}");
                    d.Start();
                }
                catch (Exception ex)
                {
                    logger.Error($"Error starting datafeed of game {d.Description}: {ex.Message}\n\n{ex.StackTrace}");
                }
            });
            activeDisplays.Where(g => !activeDisplays.Any(x => x.Description == g.Description)).ForEach(d => {
                try
                {
                    logger.Info($"Stop receiving data from {d.Description}");
                    d.Stop();
                }
                catch (Exception ex)
                {
                    logger.Error($"Error stopping datafeed of game {d.Description}: {ex.Message}\n\n{ex.StackTrace}");
                }
            });
            prevActiveDisplays = activeDisplays;
        }

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
