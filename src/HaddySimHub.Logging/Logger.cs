using NLog;

namespace HaddySimHub.Logging
{
    public class Logger : ILogger
    {
        private readonly NLog.Logger _logger = LogManager.GetCurrentClassLogger();

        public Logger(bool isDebugEnabled)
        {
            //Setup logging
            LogManager.Setup().LoadConfiguration(builder =>
            {
                builder.ForLogger().FilterMinLevel(isDebugEnabled ? LogLevel.Debug : LogLevel.Info)
                    .WriteToFile(fileName: "log.txt", layout: "${longdate} ${uppercase:${level}}: ${message}");
            });
        }

        public void Debug(string message) => this._logger.Debug(message);
        public void Error(string message) => this._logger.Error(message);

        public void Fatal(string message) => this._logger.Fatal(message);

        public void Info(string message) => this._logger.Info(message);
    }
}
