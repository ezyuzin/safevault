using System;
using SafeVault.Exceptions;

namespace SafeVault.Logger.Exceptions
{
    public class LogException : SafeVaultException
    {
        public LogException(Exception ex, string message) : base(ex, message)
        {
        }

        public LogException(string message) : base(message)
        {
        }
    }
}