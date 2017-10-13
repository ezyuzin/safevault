using System;

namespace SafeVault.Unity
{
    public class UnityException : Exception
    {
        public UnityException(Exception ex, string message) : base(message, ex)
        {
        }

        public UnityException(string message) : base(message)
        {
        }
    }
}