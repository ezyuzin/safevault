using System;

namespace SafeVault.Exceptions
{
    public class ArgumentException : SafeVaultException
    {
        public ArgumentException(Exception ex, string message) : base(ex, message)
        {
        }

        public ArgumentException(string message) : base(message)
        {
        }
    }
}