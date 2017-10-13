using System;
using SafeVault.Exceptions;

namespace SafeVault.Service.Command.Exceptions
{
    public class ContextException : SafeVaultException
    {
        public ContextException(Exception ex, string message) : base(ex, message)
        {
        }

        public ContextException(string message) : base(message)
        {
        }
    }
}