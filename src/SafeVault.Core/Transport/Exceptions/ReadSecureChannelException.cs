using System;

namespace SafeVault.Transport.Exceptions
{
    public class ReadSecureChannelException : SecureChannelException
    {
        public ReadSecureChannelException(Exception ex, string message) : base(ex, message)
        {
        }

        public ReadSecureChannelException(string message) : base(message)
        {
        }
    }
}