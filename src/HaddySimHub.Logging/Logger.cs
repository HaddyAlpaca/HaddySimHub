using NLog;

namespace HaddySimHub.Logging
{
    public class Logger : ILogger
    {
        private readonly NLog.Logger logger = LogManager.GetCurrentClassLogger();

        public Logger(bool isDebugEnabled)
        {
            // Setup logging
            LogManager.Setup().LoadConfiguration(builder =>
            {
                builder.ForLogger().FilterMinLevel(isDebugEnabled ? LogLevel.Debug : LogLevel.Info)
                    .WriteToFile(fileName: "log.txt", layout: "${longdate} ${uppercase:${level}}: ${message}");
            });
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
