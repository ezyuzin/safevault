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
using SafeVault.Exceptions;

namespace SafeVault.Security.Ciphers
{
    public class RsaCipher : IDisposable, ICipher
    {
        private RsaCipherKey _key;
        #if NETSTANDARD2_0
        public RSAEncryptionPadding RsaEncryptionPadding { get; set; }
        #endif

        public RsaCipher()
        {
            #if NETSTANDARD2_0
            RsaEncryptionPadding = RSAEncryptionPadding.Pkcs1;
            #endif
        }

        ICipher ICipher.Clone()
        {
            return this.Clone();
        }

        public int BlockSize { get { return 32; } }

        public RsaCipher Clone()
        {
            EnsureObjectNotDisposed();

            RsaCipher cipher = new RsaCipher();
#if NETSTANDARD2_0
            cipher.RsaEncryptionPadding = RsaEncryptionPadding;
#endif
            cipher._key = _key;
            cipher._key.AddRef();

            return cipher;
        }

        public void Dispose()
        {
            _key?.Dispose();
            _key = null;
        }

        protected void EnsureObjectNotDisposed()
        {
            if (_key == null)
                throw new ObjectDisposedException("Access to Disposed object");

            _key.EnsureObjectNotDisposed();
        }

        public byte[] Decrypt(byte[] data)
        {
            EnsureObjectNotDisposed();

            if (_key.Private == null)
                throw new InternalErrorException("Certificate not have private key");

            #if NETSTANDARD2_0
            return _key.Private.Decrypt(data, RsaEncryptionPadding);
            #endif

            #if NETFX
            return _key.Private.Decrypt(data, false);
            #endif
        }

        public byte[] Encrypt(byte[] data)
        {
            EnsureObjectNotDisposed();
            #if NETSTANDARD2_0
            return _key.Public.Encrypt(data, RsaEncryptionPadding);
            #endif
            
            #if NETFX
            return _key.Public.Encrypt(data, false);
            #endif
        }

        public static RsaCipher LoadFromX509Store(string friendlyName)
        {
            System.Security.Cryptography.X509Certificates.X509Store store = new System.Security.Cryptography.X509Certificates.X509Store(StoreName.My);
            try
            {
                store.Open(OpenFlags.ReadOnly);
                foreach (var x509 in store.Certificates)
                {
                    var cn = x509.FriendlyName;
                    if (cn == friendlyName)
                    {
                        var key = new RsaCipherKey();
                        try
                        {
                            #if NETSTANDARD2_0
                            key.Public = x509.GetRSAPublicKey();
                            key.Private = x509.GetRSAPrivateKey();
                            #endif

                            #if NETFX
                            key.Public = (RSACryptoServiceProvider)x509.PublicKey.Key;
                            key.Private = (RSACryptoServiceProvider)x509.PrivateKey;
                            #endif
                        }
                        catch (Exception)
                        {
                            key.Dispose();
                            throw;
                        }

                        RsaCipher rsaCipher = new RsaCipher();
                        rsaCipher._key = key;
                        return rsaCipher;
                    }
                }
            }
            finally 
            {
                #if NETSTANDARD2_0
                store.Dispose();
                #endif               
            }
            throw new InternalErrorException("Certificate not found: " + friendlyName);
        }

        public static RsaCipher LoadFromPEM(string certFilename, string keyFilename = null)
        {
            var key = new RsaCipherKey();
            try
            {
                var certificate = ReadPEM("CERTIFICATE", certFilename);
                var x509 = new X509Certificate2(certificate);
                #if NETSTANDARD2_0
                key.Public = x509.GetRSAPublicKey();
                #endif

                #if NETFX
                key.Public = (RSACryptoServiceProvider)x509.PublicKey.Key;
                #endif

                if (keyFilename != null)
                {
                    var privateKey = ReadPEM("RSA PRIVATE KEY", keyFilename);
                    RSACryptoServiceProvider prov = DecodeRsaPrivateKey(privateKey);
                    key.Private = prov;
                }
            }
            catch (Exception)
            {
                key.Dispose();
                throw;
            }
            RsaCipher rsaCipher = new RsaCipher();
            rsaCipher._key = key;
            return rsaCipher;
        }

        private static byte[] ReadPEM(string type, string filename)
        {
            using (FileStream fs = File.OpenRead(filename))
            {
                byte[] data = new byte[fs.Length];
                fs.Read(data, 0, data.Length);
                if (data[0] != 0x30)
                {
                    // maybe it's ASCII PEM base64 encoded ? 
                    data = ReadPEM(type, data);
                }
                return data;
            }
        }

        private static byte[] ReadPEM(string type, byte[] data)
        {
            string pem = Encoding.ASCII.GetString(data);
            string header = $"-----BEGIN {type}-----";
            string footer = $"-----END {type}-----";
            int start = pem.IndexOf(header, StringComparison.Ordinal) + header.Length;
            int end = pem.IndexOf(footer, start, StringComparison.Ordinal);
            string base64 = pem.Substring(start, (end - start));
            return Convert.FromBase64String(base64);
        }

        private static RSACryptoServiceProvider DecodeRsaPrivateKey(byte[] privkey)
        {
            byte[] modulus, E, D, P, Q, DP, DQ, IQ;

            // ---------  Set up stream to decode the asn.1 encoded RSA private key  ------
            MemoryStream mem = new MemoryStream(privkey);
            BinaryReader binr = new BinaryReader(mem); //wrap Memory Stream with BinaryReader for easy reading
            byte bt = 0;
            ushort twobytes = 0;
            int elems = 0;
            try
            {
                twobytes = binr.ReadUInt16();
                if (twobytes == 0x8130) //data read as little endian order (actual data order for Sequence is 30 81)
                    binr.ReadByte(); //advance 1 byte
                else if (twobytes == 0x8230)
                    binr.ReadInt16(); //advance 2 bytes
                else
                    return null;

                twobytes = binr.ReadUInt16();
                if (twobytes != 0x0102) //version number
                    return null;
                bt = binr.ReadByte();
                if (bt != 0x00)
                    return null;


                //------  all private key components are Integer sequences ----
                elems = GetIntegerSize(binr);
                modulus = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                E = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                D = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                P = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                Q = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                DP = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                DQ = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                IQ = binr.ReadBytes(elems);

                // ------- create RSACryptoServiceProvider instance and initialize with public key -----
                RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                RSAParameters rsaParams = new RSAParameters();
                rsaParams.Modulus = modulus;
                rsaParams.Exponent = E;
                rsaParams.D = D;
                rsaParams.P = P;
                rsaParams.Q = Q;
                rsaParams.DP = DP;
                rsaParams.DQ = DQ;
                rsaParams.InverseQ = IQ;
                rsa.ImportParameters(rsaParams);
                return rsa;
            }
            finally
            {
                binr.Close();
            }
        }

        private static int GetIntegerSize(BinaryReader binr)
        {
            byte bt = 0;
            var count = 0;
            bt = binr.ReadByte();
            if (bt != 0x02) //expect integer
                return 0;
            bt = binr.ReadByte();

            if (bt == 0x81)
            {
                count = binr.ReadByte(); // data size in next byte
            }
            else if (bt == 0x82)
            {
                var highbyte = binr.ReadByte();
                var lowbyte = binr.ReadByte();
                byte[] modint = {lowbyte, highbyte, 0x00, 0x00};
                count = BitConverter.ToInt32(modint, 0);
            }
            else
            {
                count = bt; // we already have the data size
            }

            while (binr.ReadByte() == 0x00) //remove high order zeros in data
            {
                count -= 1;
            }
            binr.BaseStream.Seek(-1, SeekOrigin.Current); //last ReadByte wasn't a removed zero, so back up a byte
            return count;
        }
    }
}
