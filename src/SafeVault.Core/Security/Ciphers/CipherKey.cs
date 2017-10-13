using System;

namespace SafeVault.Security.Ciphers
{
    public class CipherKey : IDisposable
    {
        private readonly object _lock = new object();
        private int _refCount = 1;

        public void AddRef()
        {
            lock (_lock)
            {
                _refCount = _refCount + 1;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        public void EnsureObjectNotDisposed()
        {
            if (_refCount == 0)
                throw new ObjectDisposedException("Access to Disposed object");
        }

        public void Dispose()
        {
            lock (_lock)
            {
                if (_refCount > 0 && (--_refCount == 0))
                {
                    Dispose(true);
                }
            }
        }
    }
}