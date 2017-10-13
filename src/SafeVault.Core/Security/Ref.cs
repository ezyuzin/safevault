using System;

namespace SafeVault.Security
{
    public class Ref<T> : IDisposable where T : IDisposable
    {
        public T Value { get; private set; }

        private readonly object _lock = new object();
        private int _refCount = 1;

        public Ref(T value)
        {
            Value = value;
        }

        public void AddRef()
        {
            lock (_lock)
            {
                _refCount = _refCount + 1;
            }
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
                    Value?.Dispose();
                    Value = default(T);
                }
            }
        }
    }
}