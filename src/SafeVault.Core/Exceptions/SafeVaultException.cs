using System;

namespace SafeVault.Exceptions
{
    public class SafeVaultException : Exception
    {
        protected SafeVaultException(Exception ex, string message) : base(message, ex)
        {
        }

        protected SafeVaultException(string message) : base(message)
        {
        }
    }
}