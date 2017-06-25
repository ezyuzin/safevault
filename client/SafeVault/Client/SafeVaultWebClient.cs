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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Script.Serialization;
using SafeVault.Configuration;
using SafeVault.Exceptions;
using SafeVault.Misc;
using SafeVault.Security;
using Random = SafeVault.Security.Random;

namespace SafeVault.Client
{
    internal class SafeVaultWebClient : IDisposable
    {
        private readonly SafeVaultConf _conf;
        public ICipherCollection<CipherName> Cipher { get; private set; }

        public string Utc { get; set; }

        public SafeVaultWebClient(SafeVaultConf conf)
        {
            _conf = conf;
            Cipher = new CipherCollection<CipherName>();
            Cipher.Add(CipherName.ServerCertificate, new Lazy<ICipher>(() => {
                var rsa = new RsaEncrypt();
                rsa.SetCertificate(conf.ServerCertificateName);
                return rsa;
            }));

            Cipher.Add(CipherName.ClientCertificate, new Lazy<ICipher>(() => {
                var rsa = new RsaEncrypt();
                rsa.SetCertificate(conf.ClientCertificateName);
                return rsa;
            }));

            Cipher.Add(CipherName.EncryptMessage, new Lazy<ICipher>(() =>
            {
                var cip1 = Cipher[CipherName.RequestPassword];
                return new CompositeCipher(cip1);
            }));

            Cipher.Add(CipherName.DecryptMessage, new Lazy<ICipher>(() =>
            {
                var cip1 = Cipher[CipherName.RequestPassword];
                var cip2 = Cipher[CipherName.ResponsePassword];
                return new CompositeCipher(cip1, cip2);
            }));
        }

        public void Dispose()
        {
            Cipher.Dispose();
        }

        public byte[] Download(string uuid, Action<int> progress = null)
        {
            var nvc = new Dictionary<string, string>();
            nvc.Add("username", _conf.Username);
            nvc.Add("password", _conf.SyncPassword.ReadString());
            nvc.Add("token", GetToken());
            nvc.Add("cmd", "dbx-download");
            nvc.Add("uuid", uuid);

            using (var httpRequest = CreateWebRequest(nvc))
            {
                var httpResponse = httpRequest.GetResponse();
                var responseJson = GetResponseJSON(httpResponse);
                var metadata = (IDictionary) responseJson["data"];

                var data = httpResponse.ReadDataChunk(int.MaxValue, CipherName.DecryptMessage, progress);
                var hash1 = GetMD5(data);
                if (hash1 != (string)metadata["md5"]) 
                    throw new SafeVaultResponseException("Checksum mismatch");

                return data;
            }
        }

        public string GetDbLastModified(string uuid)
        {
            var nvc = new Dictionary<string, string>();
            nvc.Add("username", _conf.Username);
            nvc.Add("password", _conf.SyncPassword.ReadString());
            nvc.Add("token", GetToken());
            nvc.Add("cmd", "dbx-getLastModified");
            nvc.Add("uuid", uuid);

            using (var httpRequest = CreateWebRequest(nvc))
            {
                var httpResponse = httpRequest.GetResponse();
                var responseJson = GetResponseJSON(httpResponse);
                return (string) responseJson["data"];
            }
        }

        public void Upload(string uuid, byte[] binary, string lastModified)
        {
            var nvc = new Dictionary<string, string>();
            nvc.Add("username", _conf.Username);
            nvc.Add("password", _conf.SyncPassword.ReadString());
            nvc.Add("token", GetToken());
            nvc.Add("cmd", "dbx-upload");
            nvc.Add("uuid", uuid);
            nvc.Add("last-modified", lastModified);
            nvc.Add("md5", GetMD5(binary));

            using (var httpRequest = CreateWebRequest(nvc))
            {
                httpRequest.WriteData(binary, CipherName.RequestPassword);

                var httpResponse = httpRequest.GetResponse();
                var responseJson = GetResponseJSON(httpResponse);
                var data = (string) responseJson["data"];
            }
        }

        private static string GetMD5(byte[] binary)
        {
            using (var md5 = MD5.Create())
            {
                var hash1 = md5.ComputeHash(binary);
                return string.Join("", hash1.Select(m => string.Format("{0:X2}", m)).ToArray()).ToLower();
            }
        }

        public string SetValue(string vaultKey, string vaultKeyValue, string password)
        {
            var nvc = new Dictionary<string, string>();
            nvc.Add("username", _conf.Username);
            nvc.Add("password", password);
            nvc.Add("token", GetToken());
            nvc.Add("cmd", "set-vaultkey");
            nvc.Add("vault-key", vaultKey);
            nvc.Add("vault-value", vaultKeyValue);

            using (var httpRequest = CreateWebRequest(nvc))
            {
                var httpResponse = httpRequest.GetResponse();
                var responseJson = GetResponseJSON(httpResponse);
                return (string) responseJson["data"];
            }
        }

        public string GetValue(string vaultKey, string password)
        {
            var nvc = new Dictionary<string, string>();
            nvc.Add("username", _conf.Username);
            nvc.Add("password", password);
            nvc.Add("token", GetToken());
            nvc.Add("cmd", "get-vaultkey");
            nvc.Add("vault-key", vaultKey);

            using (var httpRequest = CreateWebRequest(nvc))
            {
                var httpResponse = httpRequest.GetResponse();
                var responseJson = GetResponseJSON(httpResponse);
                return (string) responseJson["data"];
            }
        }

        public string GetToken()
        {
            var nvc = new Dictionary<string, string>();
            nvc.Add("username", _conf.Username);
            nvc.Add("cmd", "get-token");

            using (var httpRequest = CreateWebRequest(nvc))
            {
                var httpResponse = httpRequest.GetResponse();
                var responseJson = GetResponseJSON(httpResponse);
                return (string) responseJson["data"];
            }
        }

        private SafeVaultWebRequest CreateWebRequest(IDictionary nvc)
        {
            var requestPwd = Random.Get(32);

            Cipher[CipherName.RequestPassword] = new Lazy<ICipher>(() => 
            {
                var aes256 = new Aes256Encrypt();
                aes256.SetPassPhrase(requestPwd);
                return aes256;
            });

            var httpRequest = new SafeVaultWebRequest(_conf.ServerUrl, this.Cipher);
            httpRequest.WriteData(requestPwd, CipherName.ServerCertificate);

            var queryString = string.Join("\n", nvc.Keys.Cast<string>().Select(key => string.Format("{0}={1}", key, nvc[key])).ToArray());
            var data = Encoding.ASCII.GetBytes(queryString);

            httpRequest.WriteData(data, CipherName.RequestPassword);

            return httpRequest;
        }

        private IDictionary GetResponseJSON(SafeVaultWebResponse httpResponse)
        {
            var message = httpResponse.ReadDataChunk(int.MaxValue, CipherName.DecryptMessage);

            var json = Encoding.UTF8.GetString(message);
            Console.WriteLine(json);
            JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
            var response = (IDictionary) jsonSerializer.DeserializeObject(json);
            response.Remove("trash");

            Utc = (string) response["utc"];
            var status = (string)response["status"];
            if (status != "OK")
                throw new SafeVaultResponseException(status) {Status = status};

            return response;
        }
    }
}
