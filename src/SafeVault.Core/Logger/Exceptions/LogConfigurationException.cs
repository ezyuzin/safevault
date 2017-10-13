using System;
using SafeVault.Exceptions;

namespace SafeVault.Logger.Exceptions
{
    public class LogConfigurationException : LogException
    {
        public LogConfigurationException(Exception ex, string message) : base(ex, message)
        {
        }

        public LogConfigurationException(string message) : base(message)
        {
        }
    }
}