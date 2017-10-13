using System;
using SafeVault.Exceptions;

namespace SafeVault.Transport.Exceptions
{
    public class HttpChannelException : SecureChannelException
    {
        public int StatusCode { get; set; }

        public HttpChannelException(string message) : base(message)
        {
        }

        public HttpChannelException(Exception ex, string message) : base(ex, message)
        {
        }
    }
}