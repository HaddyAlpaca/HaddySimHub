namespace HaddySimHub.Server.Logging
{
    /// <summary>
    /// Logger interface.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Log debug message.
        /// </summary>
        /// <param name="message">Message to log.</param>
        void Debug(string message);

        /// <summary>
        /// Log info message.
        /// </summary>
        /// <param name="message">Message to log.</param>
        void Info(string message);

        /// <summary>
        /// Log error message.
        /// </summary>
        /// <param name="message">Message to log.</param>
        void Error(string message);

        /// <summary>
        /// Log error message.
        /// </summary>
        /// <param name="message">Message to log.</param>
        void Fatal(string message);

        /// <summary>
        /// Log data
        /// </summary>
        /// <param name="data">Data to log.</param>
        void LogData(object data);
    }
}
