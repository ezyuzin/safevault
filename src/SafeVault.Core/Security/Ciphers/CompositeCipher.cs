using System;
using System.Collections.Generic;
using System.Linq;

namespace SafeVault.Security.Ciphers
{
    public class CompositeCipher : ICipher
    {
        private List<ICipher> _list = new List<ICipher>();

        public void Dispose()
        {
            if (_list != null)
            {
                foreach (var cipher in _list)
                    cipher.Dispose();

                _list = null;
            }
        }

        public byte[] Decrypt(byte[] data)
        {
            EnsureObjectNotDisposed();
            for (int i = _list.Count; i != 0; i--)
                data = _list[i-1].Decrypt(data);

            return data;
        }

        public byte[] Encrypt(byte[] data)
        {
            EnsureObjectNotDisposed();
            foreach (var cipher in _list)
                data = cipher.Encrypt(data);

            return data;
        }

        ICipher ICipher.Clone()
        {
            return Clone();
        }

        public int BlockSize
        {
            get
            {
                return _list.Count != 0
                    ? _list.Min(m => m.BlockSize)
                    : int.MaxValue;
            }
        }

        protected void EnsureObjectNotDisposed()
        {
            if (_list == null)
                throw new ObjectDisposedException("Access to Disposed object");
        }

        public void Add(ICipher cipher)
        {
            try
            {
                EnsureObjectNotDisposed();
                _list.Add(cipher);
            }
            catch (Exception)
            {
                cipher.Dispose();
                throw;
            }
        }

        public CompositeCipher Clone()
        {
            EnsureObjectNotDisposed();

            var clone = new CompositeCipher();
            foreach(var cipher in _list)
                clone.Add(cipher.Clone());

            return clone;
        }
    }
}