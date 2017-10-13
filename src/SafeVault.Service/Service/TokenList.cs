using System;
using SafeVault.Cache;
using ArgumentException = SafeVault.Exceptions.ArgumentException;

namespace SafeVault.Service
{
    public class TokenList : IDisposable
    {
        private Cache<string, bool> _cache;

        public TokenList()
        {
            _cache = new Cache<string, bool>();
            _cache.TTL = TimeSpan.FromMinutes(30);
        }

        public void Reset()
        {
            _cache.Reset();
        }

        public void UseToken(string token)
        {
            if (string.IsNullOrEmpty(token))
                throw new ArgumentException("Xsfr token is not provided");

            if (_cache.Get(token))
                throw new ArgumentException("Used token was provided");

            _cache.Add(token, true);
        }

        public void Dispose()
        {
            _cache?.Dispose();
            _cache = null;
        }
    }
}