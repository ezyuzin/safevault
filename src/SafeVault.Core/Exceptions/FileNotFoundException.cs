using System;

namespace SafeVault.Exceptions
{
    public class FileNotFoundException : InternalErrorException
    {
        public FileNotFoundException(Exception ex, string message) : base(ex, message)
        {
        }

        public FileNotFoundException(string message) : base(message)
        {
        }
    }
}