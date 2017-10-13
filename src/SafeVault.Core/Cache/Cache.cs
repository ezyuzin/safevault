using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SafeVault.Cache
{
    public class Cache<T1, T> : IDisposable
    {
        public TimeSpan TTL { get; set; }

        private readonly Dictionary<T1, CacheItem<T>> _cache = new Dictionary<T1, CacheItem<T>>();
        private DateTime _scheduledCleanup;
        private Thread _thread;
        private readonly object _flushLock = new object();


        public Cache()
        {
            TTL = TimeSpan.FromMinutes(5);
            _scheduledCleanup = DateTime.MaxValue;
        }

        public void Reset()
        {
            lock (_cache)
            {
                _cache.Clear();
            }
        }

        public void Flush()
        {
            lock (_flushLock)
            {
                if (_scheduledCleanup < DateTime.Now && _thread == null)
                {
                    _scheduledCleanup = DateTime.MaxValue;
                    var thread = new Thread(Cleanup);
                    thread.Start();
                    _thread = thread;
                }
            }
        }

        private void Cleanup()
        {
            try
            {
                List<CacheItem<T>> values = new List<CacheItem<T>>();
                lock (_cache)
                {
                    var expired = _cache.Where(m => m.Value.IsExpired).Select(m => m).ToArray();
                    foreach (var item in expired)
                    {
                        _cache.Remove(item.Key);
                        values.Add(item.Value);
                    }
                }

                foreach (var item in values)
                    item.Dispose();
            }
            catch (Exception e)
            {
            }

            _thread = null;
        }

        public void Add(T1 name, T obj)
        {
            Add(name, obj, TTL);
        }

        public void Add(T1 name, T obj, TimeSpan ttl)
        {
            Flush();

            CacheItem<T> value = new CacheItem<T>(obj);
            value.TTL = ttl;
            lock (_cache)
            {
                _cache[name] = value;
            }

            SetLastUpdateTime(value);
        }

        private void SetLastUpdateTime(CacheItem<T> value)
        {
            value.LastAccessTime = DateTime.Now;
            var expired = value.LastAccessTime.Add(value.TTL);

            lock (_flushLock)
            {
                if (_scheduledCleanup > expired)
                {
                    _scheduledCleanup = expired;
                }
                if (_scheduledCleanup < DateTime.Now)
                {
                    Flush();
                }
            }
        }

        public T GetOrAdd(T1 name, Func<T1, T> addFunc)
        {
            return GetOrAdd(name, TTL, addFunc);
        }

        public T GetOrAdd(T1 name, TimeSpan ttl, Func<T1, T> addFunc)
        {
            Flush();

            CacheItem<T> value;
            lock (_cache)
            {
                if (!_cache.TryGetValue(name, out value) || value.IsExpired)
                {
                    value = new CacheItem<T>(addFunc(name));
                    value.TTL = ttl;
                    _cache[name] = value;
                }
            }

            SetLastUpdateTime(value);
            return value.Item;
        }

        public T Get(T1 name)
        {
            Flush();

            CacheItem<T> value;
            lock (_cache)
            {
                if (!_cache.TryGetValue(name, out value))
                    return default(T);
            }

            if (value.IsExpired)
                return default(T);

            SetLastUpdateTime(value);
            return value.Item;
        }

        public void Dispose()
        {
            _thread?.Abort();
            _thread = null;

            foreach(var value in _cache.Values)
                value.Dispose();
        }

    }
}