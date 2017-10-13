using System;

namespace SafeVault.Transport.Exceptions
{
    public class StreamControlException : SecureChannelException
    {
        public StreamControlException(Exception ex, string message) : base(ex, message)
        {
        }

        public StreamControlException(string message) : base(message)
        {
        }
    }
}