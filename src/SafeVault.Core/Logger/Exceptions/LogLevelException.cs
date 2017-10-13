using System;

namespace SafeVault.Logger.Exceptions
{
    public class LogLevelException : LogException
    {
        public LogLevelException(Exception ex, string message) : base(ex, message)
        {
        }

        public LogLevelException(string message) : base(message)
        {
        }
    }
}