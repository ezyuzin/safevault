using System;
using SafeVault.Exceptions;

namespace SafeVault.Transport.Exceptions
{
    public class SecureChannelException : SafeVaultException
    {
        public SecureChannelException(Exception ex, string message) : base(ex, message)
        {
        }

        public SecureChannelException(string message) : base(message)
        {
        }
    }
}