using System;
using SafeVault.Logger.Appender;

namespace SafeVault.Logger
{
    public interface ILogManager
    {
        ILog GetLogger(Type type);
        LogLevel LogLevel { get; set; }
        ILog GetLogger(string name);
    }
}