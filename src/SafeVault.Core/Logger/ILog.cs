namespace SafeVault.Logger
{
    public interface ILog
    {
        LogLevel LogLevel { get; set; }

        bool DebugEnabled { get; }
        bool InfoEnabled { get; }
        bool WarnEnabled { get; }
        bool ErrorEnabled { get; }
        bool FatalEnabled { get; }

        void Debug(string message, params object[] args);
        void Info(string message, params object[] args);
        void Warn(string message, params object[] args);
        void Error(string message, params object[] args);
        void Fatal(string message, params object[] args);
        void Debug(string message);
        void Info(string message);
        void Warn(string message);
        void Error(string message);
        void Fatal(string message);
        void LogRecord(LogRecord rec);
        bool IsEnabled(LogLevel level);
    }
}