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
using System.Net;
using System.Threading;
using SafeVault.Exceptions;
using SafeVault.Misc;
using SafeVault.Security;

namespace SafeVault.Client
{
    internal class SafeVaultWebResponse : IDisposable
    {
        public ICipherCollection<CipherName> Ciphers { get; }
        private readonly Stream _stream;

        public SafeVaultWebResponse(HttpWebResponse response, ICipherCollection<CipherName> ciphers)
        {
            Ciphers = ciphers;

            var response1 = response;
            if (response1.ContentLength > 16*1024*1024)
                throw new SafeVaultResponseException("BadResponse: Invalid Protocol");

            if (response1.ContentLength == 0)
                throw new SafeVaultResponseException("No Content");

            _stream = response.GetResponseStream();

            if (_stream == null)
                throw new SafeVaultResponseException("No Content");

            var responsePwd = ReadDataChunk(256, CipherName.ClientCertificate);
            Ciphers[CipherName.ResponsePassword] = new Lazy<ICipher>(() =>
            {
                var aes = new Aes256Encrypt();
                aes.SetPassPhrase(responsePwd);
                return aes;
            });
        }

        public void Dispose()
        {
            if (_stream != null)
                _stream.Dispose();
        }

        public byte[] ReadDataChunk(int maxLen, CipherName cipherName, Action<int> progress = null)
        {
            var bLen = new byte[4];
            if (_stream.Read(bLen, 0, 4) != 4)
                throw new SafeVaultResponseException("BadResponse: Invalid Protocol");

            var dataLen = BitConverter.ToInt32(bLen, 0);
            if (dataLen > maxLen)
                throw new SafeVaultResponseException("BadResponse: Invalid Protocol");

            var data = new byte[dataLen];
            var npos = 0;

            DateTime lastDate = DateTime.Now;
            while (dataLen > 0)
            {
                var nread = _stream.Read(data, npos, dataLen);
                if (nread == 0 &&  DateTime.Now > lastDate.AddSeconds(10))
                    throw new SafeVaultResponseException("BadResponse: Read Timeout");

                if (nread != 0)
                    lastDate = DateTime.Now;

                npos = npos + nread;
                dataLen = dataLen - nread;
                if (dataLen == 0)
                    break;

                if (nread == 0)
                {
                    if (progress != null)
                    {
                        progress.Invoke(npos * 100 / data.Length);
                    }
                    Thread.Sleep(5);
                }
            }

            try
            {
                return Ciphers[cipherName].Value.Decrypt(data);
            }
            catch(SafeVaultResponseException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new SafeVaultResponseException(e, "BadResponse: Invalid Protocol:" + e.Message);
            }
        }
    }
}