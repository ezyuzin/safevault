using System;

namespace SafeVault.Cache
{
    internal class CacheItem<T> : IDisposable
    {
        public DateTime LastAccessTime { get; set; }
        public TimeSpan TTL { get; set; }

        public T Item { get; private set; }

        public CacheItem(T obj)
        {
            Item = obj;
            LastAccessTime = DateTime.Now;
        }

        public bool IsExpired
        {
            get
            {
                return (DateTime.Now >= LastAccessTime.Add(TTL));
            }
        }

        public void Dispose()
        {
            (Item as IDisposable)?.Dispose();
            Item = default(T);
        }
    }
}