using System;
using SafeVault.Exceptions;

namespace SafeVault.Configuration.Exceptions
{
    public class ConfigurationException : SafeVaultException
    {
        public ConfigurationException(Exception ex, string message) : base(ex, message)
        {
        }

        public ConfigurationException(string message) : base(message)
        {
        }
    }
}