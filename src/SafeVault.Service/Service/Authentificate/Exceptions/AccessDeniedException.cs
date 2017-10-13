using System;

namespace SafeVault.Exceptions
{
    public class AccessDeniedException : BadRequestException
    {
        public AccessDeniedException(Exception ex, string message) : base(ex, message)
        {
        }

        public AccessDeniedException(string message) : base(message)
        {
        }
    }
}