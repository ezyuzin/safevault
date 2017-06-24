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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web.Script.Serialization;
using SafeVaultKeyPlugin.Encrypt;
using SafeVaultKeyProv;

namespace SafeVaultKeyPlugin
{
    public delegate void VaultQueryCallback(string message);

    internal class VaultQuery
    {
        public event VaultQueryCallback Callback;

        public string VaultUsername { get; set; }
        public string VaultKeyname { get; set; }

        public string ServerUrl { get; set; }
        public string ServerCertificateName { get; set; }
        public string ClientCertificateName { get; set; }

        public string Utc { get; set; }

        public VaultQuery()
        {
        }

        public VaultQuery(VaultConf conf)
        {
            VaultUsername = conf.Data.VaultUsername;
            VaultKeyname = conf.Data.VaultKeyname;
            ServerCertificateName = conf.Data.ServerCertificateName;
            ServerUrl = conf.Data.ServerUrl;
            ClientCertificateName = conf.Data.ClientCertificateName;
        }

        private void Status(string message)
        {
            if (Callback != null)
                Callback.Invoke(message);
        }

        public string SetValue(string password, string value)
        {
            var nvc = new Dictionary<string, string>();
            nvc.Add("username", VaultUsername);
            nvc.Add("password", password);
            nvc.Add("set-vaultkey", VaultKeyname);
            nvc.Add("set-vaultvalue", value);

            return GetValueInternal(nvc);
        }

        public string GetValue(string password)
        {
            var nvc = new Dictionary<string, string>();
            nvc.Add("username", VaultUsername);
            nvc.Add("password", password);
            nvc.Add("get-vaultkey", VaultKeyname);

            return GetValueInternal(nvc);
        }

        public string GetValueInternal(IDictionary nvc)
        {
            /*
            request: 
                dataEnc = enc(data, reqPwd);
                reqPwdEnc = enc(reqPwd, server_cert)
                -
                1) hacker need have a server privatekey to decrypt request

            response: 
                dataEnc = enc(data, passw)
                passwEnc = enc(passw, client_cert)
                dataEnc = enc(dataEnc, request.response_pwd)
                passwEnc = enc(passwEnc, request.response_pwd)
            
                1) hacker need have a server private key to unpack request and get response password
                2) hacker need have a client private key to decrypt passw
             */


            RsaEncrypt serverKey = new RsaEncrypt();
            serverKey.SetCertificate(ServerCertificateName);

            var requestPwd = VaultKeyProvider.GetRandom(32);
            var responsePwd = VaultKeyProvider.GetRandom(256);

            nvc.Add("responsepwd", Convert.ToBase64String(responsePwd));
            var queryString = string.Join("\n", nvc.Keys.Cast<string>().Select(key => string.Format("{0}={1}", key, nvc[key])).ToArray());

            string datab64;
            using (var aes = new Aes256Encrypt())
            {
                aes.SetPassPhrase(requestPwd);
                var enc1 = aes.Encrypt(Encoding.ASCII.GetBytes(queryString));
                datab64 = Convert.ToBase64String(enc1);
            }

            var pwdb64 = Convert.ToBase64String(serverKey.Encrypt(requestPwd));

            var httpRequest = (HttpWebRequest)WebRequest.Create(string.Format(ServerUrl, datab64, pwdb64));
            httpRequest.Proxy = WebRequest.GetSystemWebProxy();
            if (httpRequest.Proxy != null)
                httpRequest.Proxy.Credentials = CredentialCache.DefaultNetworkCredentials;

            var response = httpRequest.GetResponse();
            using (var responseStream = new StreamReader(response.GetResponseStream()))
            {
                var json = responseStream.ReadToEnd();

                JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
                IDictionary json1 = (IDictionary)jsonSerializer.DeserializeObject(json);

                Utc = (string)json1["utc"];

                byte[] data = Convert.FromBase64String((string)json1["a"]);
                byte[] password = Convert.FromBase64String((string)json1["b"]);

                string debugMsg = (string)json1["debug"];
                if (!string.IsNullOrEmpty(debugMsg)) {
                    debugMsg = debugMsg.Trim(';').Trim();
                    if (debugMsg != "OK")
                    {
                        throw new SafeVaultException("Remote: " + debugMsg);
                    }
                }

                string phase = "";
                try
                {
                    using (RsaEncrypt clientCertificate = new RsaEncrypt())
                    {
                        clientCertificate.SetCertificate(ClientCertificateName);

                        phase = "Decrypt password1";
                        password = clientCertificate.Decrypt(password);
                    }

                    using (var aes = new Aes256Encrypt())
                    {
                        phase = "SetPassPhrase1";
                        aes.SetPassPhrase(responsePwd);

                        phase = "Decrypt password2";
                        password = aes.Decrypt(password);
                        phase = "Decrypt data1";
                        data = aes.Decrypt(data);
                    }

                    using (var aes = new Aes256Encrypt())
                    { 
                        phase = "SetPassPhrase2";
                        aes.SetPassPhrase(password);                        
                        phase = "Decrypt data2";
                        data = aes.Decrypt(data);
                        return Encoding.UTF8.GetString(data);
                    }
                }
                catch (CryptographicException ex)
                {
                    throw new SafeVaultException(phase + " " + ex.Message);
                }
            }
        }
    }
}
