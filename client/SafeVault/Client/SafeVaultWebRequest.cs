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
using SafeVault.Exceptions;
using SafeVault.Security;

namespace SafeVault.Client
{
    internal class SafeVaultWebRequest : IDisposable
    {
        public ICipherCollection<CipherName> Cipher { get; private set; }

        private MemoryStream _requestData = new MemoryStream();
        private readonly string _requestUri;
        private SafeVaultWebResponse _response;

        public SafeVaultWebRequest(string requestUri, ICipherCollection<CipherName> cipher)
        {
            _requestUri = requestUri;
            Cipher = cipher;
        }

        public void Dispose()
        {
            if (_response != null)
            {
                _response.Dispose();
                _response = null;
            }

            if (_requestData != null)
            {
                _requestData.Dispose();
                _requestData = null;
            }
        }

        public SafeVaultWebResponse GetResponse()
        {
            if (_response != null)
                return _response;

            _requestData.Position = 0;

            var httpRequest = (HttpWebRequest) WebRequest.Create(_requestUri);
            httpRequest.Method = "POST";

            httpRequest.Proxy = WebRequest.GetSystemWebProxy();
            if (httpRequest.Proxy != null)
                httpRequest.Proxy.Credentials = CredentialCache.DefaultCredentials;

            httpRequest.ContentType = "application/x-www-form-urlencoded";
            httpRequest.ContentLength = _requestData.Length;

            var buffer = new byte[65535];
            using (var stream = httpRequest.GetRequestStream())
            {
                int count;
                while ((count = _requestData.Read(buffer, 0, buffer.Length)) > 0)
                    stream.Write(buffer, 0, count);

                stream.Flush();
            }
            _requestData.Dispose();
            _requestData = null;

            /*
            response: 
                dataEnc = enc(data, passw)
                passwEnc = enc(passw, client_cert)
                dataEnc = enc(dataEnc, request.response_pwd)
                passwEnc = enc(passwEnc, request.response_pwd)
            
                1) hacker need have a server private key to unpack request and get response password
                2) hacker need have a client private key to decrypt passw
             */

            try
            {
                var httpWebResponse = (HttpWebResponse) httpRequest.GetResponse();
                _response = new SafeVaultWebResponse(httpWebResponse, Cipher);
                return _response;
            }
            catch (WebException e)
            {
                using (var response = (HttpWebResponse) e.Response)
                {
                    if (response == null)
                        throw new SafeVaultHttpException(e.Status + ": " + e.Message);

                    throw new SafeVaultHttpException(response.StatusCode + ": " + e.Message)
                    {
                        StatusCode = response.StatusCode
                    };
                }
            }
        }

        public void WriteData(byte[] data, CipherName cipherName)
        {
            data = Cipher[cipherName].Value.Encrypt(data);

            var pwdLen = BitConverter.GetBytes(data.Length);
            _requestData.Write(pwdLen, 0, pwdLen.Length);
            _requestData.Write(data, 0, data.Length);
        }
    }
}