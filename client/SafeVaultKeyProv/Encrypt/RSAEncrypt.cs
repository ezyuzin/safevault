/*
  SafeVaultKeyProvider Plugin
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
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace SafeVaultKeyPlugin.Encrypt
{
    public class RsaEncrypt : IDisposable
    {
        public X509Certificate2 X509 { get; private set; }

        public void SetCertificate(string friendlyName)
        {
            X509Store store = new X509Store(StoreName.My);
            store.Open(OpenFlags.ReadOnly);
            foreach (X509Certificate2 x509 in store.Certificates)
            {
                var cn = x509.FriendlyName;
                if (cn == friendlyName)
                {
                    X509 = x509;
                    return;
                }
            }
            throw new SafeVaultException("Certificate not found: " + friendlyName);
        }

        public void LoadPEMCertificate(string filename, string type)
        {
            X509Certificate2 x509 = null;
            using (FileStream fs = File.OpenRead(filename))
            {
                byte[] data = new byte[fs.Length];
                fs.Read(data, 0, data.Length);
                if (data[0] != 0x30)
                {
                    // maybe it's ASCII PEM base64 encoded ? 
                    data = LoadPEM(type, data);
                }
                if (data != null)
                    x509 = new X509Certificate2(data);
            }

            X509 = x509;
        }

        public static byte[] LoadPEM(string type, byte[] data)
        {
            string pem = Encoding.ASCII.GetString(data);
            string header = $"-----BEGIN {type}-----";
            string footer = $"-----END {type}-----";
            int start = pem.IndexOf(header, StringComparison.Ordinal) + header.Length;
            int end = pem.IndexOf(footer, start, StringComparison.Ordinal);
            string base64 = pem.Substring(start, (end - start));
            return Convert.FromBase64String(base64);
        }

        public byte[] Decrypt(byte[] data)
        {
            var privateKey = (RSACryptoServiceProvider)X509.PrivateKey;
            return privateKey.Decrypt(data, false);
        }

        public byte[] Encrypt(byte[] data)
        {
            var publicKey = (RSACryptoServiceProvider)X509.PublicKey.Key;
            return publicKey.Encrypt(data, false);
        }

        public void Dispose()
        {
            X509 = null;
        }
    }
}
