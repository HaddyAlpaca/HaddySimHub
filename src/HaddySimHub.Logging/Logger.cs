using NLog;
using NLog.Config;
using NLog.Targets;

namespace HaddySimHub.Logging
{
    public class Logger : ILogger
    {
        private readonly NLog.Logger logger = LogManager.GetCurrentClassLogger();

        public Logger(bool isDebugEnabled)
        {
            // Setup logging
            var logConfig = new LoggingConfiguration();

            if (isDebugEnabled)
            {
                // Debug
                var debugTarget = new FileTarget
                {
                    FileName = "HaddySimHub.debug.log",
                    Layout = @"--START--\n${message}\n--END--\n",
                    ArchiveAboveSize = 1_000_000_000,
                    ArchiveNumbering = ArchiveNumberingMode.DateAndSequence,
                    MaxArchiveDays = 1,
                };

                logConfig.LoggingRules.Add(new LoggingRule(
                    "*",
                    LogLevel.Debug,
                    LogLevel.Debug,
                    debugTarget));
                logConfig.AddTarget("debug-logfile", debugTarget);
            }

            // General
            var fileTarget = new FileTarget
            {
                FileName = "HaddySimHub.log",
                Layout = @"${longdate} ${uppercase:${level}}: ${message}",
                ArchiveAboveSize = 1_000_000_000,
                ArchiveNumbering = ArchiveNumberingMode.DateAndSequence,
                MaxArchiveDays = 1,
            };

            logConfig.LoggingRules.Add(new LoggingRule(
                "*",
                LogLevel.Info,
                LogLevel.Fatal,
                fileTarget));
            logConfig.AddTarget("general-logfile", fileTarget);

            LogManager.Configuration = logConfig;
        }

        /// <inheritdoc/>
        public void Debug(string message) => this.logger.Debug(message);

        /// <inheritdoc/>
        public void Error(string message) => this.logger.Error(message);

        /// <inheritdoc/>
        public void Fatal(string message) => this.logger.Fatal(message);

        /// <inheritdoc/>
        public void Info(string message) => this.logger.Info(message);
    }
}
