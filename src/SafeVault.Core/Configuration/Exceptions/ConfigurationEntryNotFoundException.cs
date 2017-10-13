using System;

namespace SafeVault.Configuration.Exceptions
{
    public class ConfigurationEntryNotFoundException : ConfigurationException
    {
        public ConfigurationEntryNotFoundException(Exception ex, string message) : base(ex, message)
        {
        }

        public ConfigurationEntryNotFoundException(string message) : base(message)
        {
        }
    }
}