using Microsoft.Extensions.Hosting;
using NLog;
using NLog.Config;
using NLog.Targets;
using HaddySimHub;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.Diagnostics;
using HaddySimHub.Runners;
using Logger = HaddySimHub.Logger;

// Ensure single instance of the application
Mutex mutex = new(true, "HaddySimHub_SingleInstance", out bool createdNew);
if (!createdNew)
{
    Logger.Error("Another instance HaddySimHub is already running.");
    return;
}

// Setup logging
bool isDebugEnabled = args.Contains("--debug");
Logger.Setup(isDebugEnabled);

//Check for updates
try
{
    if (await Updater.UpdateAvailable())
    {
        Logger.Info("Update available. Starting updater...");
        Updater.Update();
    }
}
catch (Exception ex)
{
    Logger.Error($"Error checking for updates: {ex.Message}\n\n{ex.StackTrace}");
}

CancellationTokenSource cancellationTokenSource = new();
CancellationToken token = cancellationTokenSource.Token;
JsonSerializerOptions serializeOptions = new() { IncludeFields = true };

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

var testRun = args.Contains("--test-run");
var processTask = new Task(async () =>
{
    // Monitor processes
    IRunner runner;
    if (testRun)
    {
        runner = new TruckTestRunner();
    }
    else
    {
        runner = new DisplaysRunner();
    }

    await runner.RunAsync(token);
}, token);


try
{
    webServerTask.Start();
    processTask.Start();
}
catch (Exception ex)
{
    Logger.Fatal($"Unhandled Exception: {ex.Message}\n\n{ex.StackTrace}");

    //Restart on crash
    var applicationPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
    Process.Start(applicationPath);
    Environment.Exit(Environment.ExitCode);
}

webServer.WaitForShutdown();
cancellationTokenSource.Cancel();
Task.WaitAll([webServerTask, processTask]);
