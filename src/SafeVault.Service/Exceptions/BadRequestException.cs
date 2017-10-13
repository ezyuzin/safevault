using System;

namespace SafeVault.Exceptions
{
    public class BadRequestException : ArgumentException
    {
        public BadRequestException(Exception ex, string message) : base(ex, message)
        {
        }

        public BadRequestException(string message) : base(message)
        {
        }
    }
}