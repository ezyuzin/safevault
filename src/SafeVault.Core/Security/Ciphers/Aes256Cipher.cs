/*
  SafeVault KeePass Plugin
  Copyright (C) 2016-2017 Evgeny Zyuzin <evgeny.zyuzin@gmail.com>

  This program is free software; you can redistribute it and/or modify
  it under the terms of the GNU General Public License as published by
  the Free Software Foundation; either version 2 of the License, or
  (at your option) any later version.

  This program is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU General Public License for more details.

  You should have received a copy of the GNU General Public License
  along with this program; if not, write to the Free Software
  Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
*/

using System;
using System.IO;
using System.Security.Cryptography;

namespace SafeVault.Security.Ciphers
{
    public class Aes256Cipher : ICipher
    {
        private AesCipherKey _key;

        public Aes256Cipher()
        {
            _key = new AesCipherKey(new RijndaelManaged
            {
                KeySize = 256,
                BlockSize = 128,
                Mode = CipherMode.CBC
            });
        }

        public Aes256Cipher(byte[] password) : this()
        {
            SetPassPhrase(password);
        }

        ICipher ICipher.Clone()
        {
            return this.Clone();
        }

        public int BlockSize { get { return ushort.MaxValue - 16; } }

        public Aes256Cipher Clone()
        {
            EnsureObjectNotDisposed();

            var cipher = new Aes256Cipher();
            cipher._key = _key;
            cipher._key.AddRef();

            return cipher;
        }

        private byte[] Join(Array array1, Array array2)
        {
            byte[] result = new byte[array1.Length + array2.Length];
            Array.Copy(array1, 0, result, 0, array1.Length);
            Array.Copy(array2, 0, result, array1.Length, array2.Length);

            return result;
        }

        public void SetPassPhrase(byte[] password)
        {
            EnsureObjectNotDisposed();

            using (var md5 = MD5.Create())
            {
                var hash1 = md5.ComputeHash(password);
                var hash2 = md5.ComputeHash(Join(hash1, password));
                var hash3 = md5.ComputeHash(Join(hash2, password));

                _key.Aes.Key = Join(hash1, hash2);
                _key.Aes.IV = hash3;
            }
        }

        public byte[] Decrypt(byte[] data)
        {
            EnsureObjectNotDisposed();

            using (var input = new MemoryStream(data))
            using (var output = new MemoryStream())
            {
                Decrypt(input, output);
                return output.ToArray();
            }
        }

        private void CopyTo(Stream a, Stream b)
        {
            byte[] buf = new byte[4096];
            while (true)
            {
                int nbytes = a.Read(buf, 0, buf.Length);
                if (nbytes == 0)
                    break;

                b.Write(buf, 0, nbytes);
            }
        }

        public void Decrypt(Stream input, Stream output)
        {
            EnsureObjectNotDisposed();

            using(var decryptor = _key.Aes.CreateDecryptor())
            using (var cs = new CryptoStream(input, decryptor, CryptoStreamMode.Read))
            {
                CopyTo(cs, output);
            }
        }

        public void Encrypt(Stream input, Stream output)
        {
            EnsureObjectNotDisposed();

            using(var encryptor = _key.Aes.CreateEncryptor())
            using (var cs = new CryptoStream(output, encryptor, CryptoStreamMode.Write))
            {
                CopyTo(input, cs);
                cs.Close();
            }
        }

        public byte[] Encrypt(byte[] data)
        {
            EnsureObjectNotDisposed();

            using (var input = new MemoryStream(data))
            using (var output = new MemoryStream())
            {
                Encrypt(input, output);
                return output.ToArray();
            }
        }

        protected void EnsureObjectNotDisposed()
        {
            if (_key == null)
                throw new ObjectDisposedException("Access to Disposed object");

            _key.EnsureObjectNotDisposed();
        }

        public void Dispose()
        {
            _key?.Dispose();
            _key = null;
        }
    }
}
