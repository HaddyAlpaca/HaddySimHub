using System.Text.Json;
using HaddySimHub.Dashboard;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace HaddySimHub;

public static class Logger
{
    private static readonly NLog.Logger _logger = LogManager.GetLogger("HaddySimHub");

    /// <inheritdoc/>
    public static void Debug(string message) => _logger.Debug(message);

    /// <inheritdoc/>
    public static void Error(string message) => _logger.Error(message);

    /// <inheritdoc/>
    public static void Fatal(string message) => _logger.Fatal(message);

    /// <inheritdoc/>
    public static void Info(string message) => _logger.Info(message);

    public static void LogData(object data) => _logger.Trace($"{JsonSerializer.Serialize(data)}\n");

    private static bool _dashboardConsole;

    public static void Setup()
    {
        _dashboardConsole = ConsoleDashboard.IsSupported;
        Configure(_dashboardConsole);
    }

    /// <summary>
    /// Switches console logging from the live dashboard back to the classic
    /// coloured console output. Used when the dashboard fails so that log
    /// messages remain visible to the operator.
    /// </summary>
    public static void ActivateConsoleFallback()
    {
        if (!_dashboardConsole)
        {
            return;
        }

        _dashboardConsole = false;
        Configure(useDashboardConsole: false);
    }

    private static void Configure(bool useDashboardConsole)
    {
        // Check environment variable voor debug logging
        var enableDebugLogging = Environment.GetEnvironmentVariable("HADDYSIMHUB_DEBUG") == "1";

        var logConfig = new LoggingConfiguration();

        // When attached to an interactive terminal, route console output into the
        // live dashboard instead of writing raw log lines. Otherwise keep the
        // classic coloured console output (e.g. CI or redirected output).
        Target consoleTarget = useDashboardConsole
            ? new DashboardLogTarget(DashboardLogStore.Instance) { Layout = @"${message}" }
            : new ColoredConsoleTarget { Layout = @"${message}" };

        logConfig.LoggingRules.Add(new LoggingRule(
            "*",
            enableDebugLogging ? LogLevel.Debug : LogLevel.Info,
            LogLevel.Fatal,
            consoleTarget));
        logConfig.AddTarget("console", consoleTarget);

        if (enableDebugLogging)
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
            enableDebugLogging ? LogLevel.Debug : LogLevel.Info,
            LogLevel.Fatal,
            fileTarget));
        logConfig.AddTarget("general-logfile", fileTarget);

        LogManager.Configuration = logConfig;
    }
}
