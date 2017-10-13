using System;

namespace SafeVault.Configuration.Exceptions
{
    public class ConfigurationMultiEntryException : ConfigurationException
    {
        public ConfigurationMultiEntryException(Exception ex, string message) : base(ex, message)
        {
        }

        public ConfigurationMultiEntryException(string message) : base(message)
        {
        }
    }
}