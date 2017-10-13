using System;
using System.Collections.Generic;
using SafeVault.Security.Ciphers;

namespace SafeVault.Security
{
    public class CipherCollection : IDisposable
    {
        private Dictionary<string, ICipher> _ciphers = new Dictionary<string, ICipher>();

        public bool ContainsKey(string key)
        {
            return _ciphers.ContainsKey(key);
        }

        public void Remove(string key)
        {
            if (_ciphers.ContainsKey(key))
            {
                _ciphers[key].Dispose();
                _ciphers.Remove(key);
            }
        }

        public ICipher this[string key]
        {
            get { return _ciphers[key]; }
            set 
            {
                if (_ciphers.ContainsKey(key))
                {
                    _ciphers[key].Dispose();
                }
                _ciphers[key] = value;
            }
        }

        public void Dispose()
        {
            foreach(var value in _ciphers.Values)
                value.Dispose();

            _ciphers = null;
        }
    }
}