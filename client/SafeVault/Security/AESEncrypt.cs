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
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace SafeVault.Security
{
    public class Aes256Encrypt : IDisposable, ICipher
    {
        private readonly RijndaelManaged _aes;

        public byte[] Key { get { return _aes.Key; } }
        public byte[] IV { get { return _aes.IV; } }

        public Aes256Encrypt()
        {
            _aes = new RijndaelManaged();
            _aes.KeySize = 256;
            _aes.BlockSize = 128;
            _aes.Mode = CipherMode.CBC;
        }

        public byte[] Join(Array array1, Array array2)
        {
            byte[] result = new byte[array1.Length + array2.Length];
            Array.Copy(array1, 0, result, 0, array1.Length);
            Array.Copy(array2, 0, result, array1.Length, array2.Length);

            return result;
        }

        public void SetPassPhrase(byte[] password)
        {
            using (var md5 = MD5.Create())
            {
                var hash1 = md5.ComputeHash(password);
                var hash2 = md5.ComputeHash(Join(hash1, password));
                var hash3 = md5.ComputeHash(Join(hash2, password));

                _aes.Key = Join(hash1, hash2);
                _aes.IV = hash3;
            }
        }

        public byte[] Decrypt(byte[] data)
        {
            using (var ms = new MemoryStream(data))
            {
                using (var cs = new CryptoStream(ms, _aes.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    List<byte> data1 = new List<byte>();
                    while (true)
                    {
                        int byte1 = cs.ReadByte();
                        if (byte1 == -1)
                            break;

                        data1.Add((byte)byte1);
                    }
                    return data1.ToArray();
                }
            }
        }

        public byte[] Encrypt(byte[] data)
        {
            using (var ms = new MemoryStream())
            {
                using (var cs = new CryptoStream(ms, _aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(data, 0, data.Length);
                    cs.Close();
                }
                return ms.ToArray();
            }
        }

        public void Dispose()
        {
            //_aes.Dispose();
        }
    }
}
