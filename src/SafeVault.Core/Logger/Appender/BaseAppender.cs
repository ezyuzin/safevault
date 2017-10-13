using System;
using SafeVault.Configuration;

namespace SafeVault.Logger.Appender
{
    public abstract class BaseAppender : ILogAppender
    {
        public string Name { get; private set; }

        public LogLevel LogLevel { get; private set; }

        public abstract void LogRecord(LogRecord message);

        public BaseAppender()
        {
            LogLevel = LogLevel.Debug;
        }

        public virtual void Configure(IConfigSection section)
        {
            Name = section.Get("@name", false);
            Name = Name ?? (GetType().Name.Replace("Appender", "").ToLower());

            var level = section.Get("level", false) ?? "DEBUG";
            LogLevel = (LogLevel)Enum.Parse(typeof(LogLevel), level, true);
        }

        public bool IsLevelEnabled(LogLevel level)
        {
            return (LogLevel <= level);
        }
    }
}