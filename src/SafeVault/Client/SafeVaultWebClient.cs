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
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using SafeVault.Configuration;
using SafeVault.Exceptions;
using SafeVault.Security;
using SafeVault.Security.Ciphers;
using SafeVault.Transport;
using SafeVault.Transport.Exceptions;
using SafeVault.Transport.Models;

namespace SafeVault.Client
{
    internal class SafeVaultWebClient
    {
        private readonly SafeVaultConf _conf;

        public string Utc { get; set; }

        public SafeVaultWebClient(SafeVaultConf conf)
        {
            _conf = conf;
        }

        public byte[] DbxDownload(string uuid, Action<int> progress = null)
        {
            QueryMessage qm = new QueryMessage();
            qm.Command = "dbx-Download";
            qm.XsfrToken = Guid.NewGuid().ToString();

            qm.Params.Add("username", _conf.Username);
            qm.Params.Add("password", _conf.SyncPassword.ReadString());
            qm.Params.Add("uuid", uuid);

            using (var channel = CreateHttpServiceChannel())
            {
                channel.Encrypt(true);
                channel.WriteObject(qm);
                channel.Post();

                var response = GetResponseMessage(channel);
                var data = channel.Read();
                var hash1 = Hash.MD5(data);
                if (hash1 != (string)response.Header["md5"])
                    throw new SecureChannelException("Invalid Checksum");

                return data;
            }
        }

        public DateTime DbxGetLastModified(string uuid)
        {
            QueryMessage qm = new QueryMessage();
            qm.Command = "dbx-GetLastModified";
            qm.XsfrToken = Guid.NewGuid().ToString();

            qm.Params.Add("username", _conf.Username);
            qm.Params.Add("password", _conf.SyncPassword.ReadString());
            qm.Params.Add("uuid", uuid);

            using (var channel = PostRequest(qm))
            {
                var response = GetResponseMessage(channel);
                return DateTime.Parse(response.Header["data"]);
            }
        }

        public void DbxUpload(string uuid, byte[] binary, DateTime lastModified)
        {
            QueryMessage qm = new QueryMessage();
            qm.Command = "dbx-Upload";
            qm.Params.Add("username", _conf.Username);
            qm.Params.Add("password", _conf.SyncPassword.ReadString());
            qm.Params.Add("uuid", uuid);
            qm.Params.Add("last-modified", lastModified.ToString("u"));
            qm.Params.Add("md5", Hash.MD5(binary));

            using (var channel = CreateHttpServiceChannel())
            {
                channel.Encrypt(true);
                channel.WriteObject(qm);
                channel.Write(binary);
                channel.Post();

                GetResponseMessage(channel);
            }
        }

        public string SetDbxKey(string uuid, string vaultKeyValue, string password)
        {
            QueryMessage qm = new QueryMessage();
            qm.Command = "dbx-SetKey";
            qm.Params.Add("username", _conf.Username);
            qm.Params.Add("password", password);
            qm.Params.Add("uuid", uuid);
            qm.Params.Add("value", vaultKeyValue);

            using (var channel = PostRequest(qm))
            {
                var response = GetResponseMessage(channel);
                return response.Header["data"];
            }
        }

        public string GetDbxKey(string vaultKey, string password)
        {
            QueryMessage qm = new QueryMessage();
            qm.Command = "dbx-GetKey";
            qm.Params["username"] = _conf.Username;
            qm.Params["password"] = password;
            qm.Params["uuid"] = vaultKey;

            using (var channel = PostRequest(qm))
            {
                var response = GetResponseMessage(channel);
                return response.Header["data"];
            }
        }

        private HttpServiceChannel PostRequest<T>(T obj)
        {
            var channel = CreateHttpServiceChannel();
            try
            {
                channel.Encrypt(true);

                channel.WriteObject(obj);
                channel.Post();
                return channel;
            }
            catch (Exception)
            {
                channel.Dispose();
                throw;
            }
        }

        private static ResponseMessage GetResponseMessage(HttpServiceChannel channel)
        {
            var response = channel.ReadObject<ResponseMessage>();
            if (response.StatusCode != (int)HttpStatusCode.OK)
                throw new HttpChannelException(response.StatusText) {StatusCode = (int) response.StatusCode};

            if ((DateTime.UtcNow - response.Timestamp).TotalMinutes > 5)
            {
                throw new HttpChannelException("Invalid Response Time");
            }
            return response;
        }

        private HttpServiceChannel CreateHttpServiceChannel()
        {
            HttpServiceChannel channel = null;
            try
            {
                channel = new HttpServiceChannel(new Uri(_conf.ServerUrl));
                channel.CipherLib["rsa-private"] = RsaCipher.LoadFromX509Store(_conf.ClientCertificateName);
                channel.CipherLib["rsa-public"] = RsaCipher.LoadFromX509Store(_conf.ServerCertificateName);
            }
            catch (Exception)
            {
                channel?.Dispose();
                throw;
            }

            return channel;
        }
    }
}
