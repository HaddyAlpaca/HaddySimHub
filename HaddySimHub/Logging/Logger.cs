using System.Text.Json;
using NLog;

namespace HaddySimHub.Logging
{
    public class Logger(string name) : ILogger
    {
        private readonly NLog.Logger _logger = LogManager.GetLogger(name);

        /// <inheritdoc/>
        public void Debug(string message) => this._logger.Debug(message);

        /// <inheritdoc/>
        public void Error(string message) => this._logger.Error(message);

        /// <inheritdoc/>
        public void Fatal(string message) => this._logger.Fatal(message);

        /// <inheritdoc/>
        public void Info(string message) => this._logger.Info(message);

        public void LogData(object data) => this._logger.Trace($"{JsonSerializer.Serialize(data)}\n");
    }
}
