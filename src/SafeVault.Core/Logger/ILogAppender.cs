using SafeVault.Configuration;

namespace SafeVault.Logger
{
    public interface ILogAppender
    {
        void LogRecord(LogRecord message);
        void Configure(IConfigSection section);
        string Name { get; }
        bool IsLevelEnabled(LogLevel level);
    }
}