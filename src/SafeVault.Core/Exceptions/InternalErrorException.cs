using System;

namespace SafeVault.Exceptions
{
    public class InternalErrorException : SafeVaultException
    {
        public InternalErrorException(Exception ex, string message) : base(ex, message)
        {
        }

        public InternalErrorException(string message) : base(message)
        {
        }
    }
}