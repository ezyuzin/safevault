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
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using KeePass.UI;
using KeePassLib.Keys;
using KeePassLib.Utility;
using SafeVault.Client;
using SafeVault.Configuration;
using SafeVault.Exceptions;
using SafeVault.Forms;
using SafeVault.Misc;
using SafeVault.Security;
using SafeVault.Security.Ciphers;
using Random = SafeVault.Security.Random;
using SafeVaultException = SafeVault.Exceptions.SafeVaultException;

namespace SafeVault
{
    public sealed class SafeVaultKeyProvider : KeyProvider
    {
        private const string PROVIDER_TYPE = "SAFE-VAULT";
        private const string PROVIDER_VERSION = "1.0"; // File version, not OtpKeyProv version

        public override string Name
        {
            get { return "SAFE-VAULT"; }
        }

        public override bool SecureDesktopCompatible
        {
            get { return true; }
        }

        public override byte[] GetKey(KeyProviderQueryContext ctx)
        {
            try
            {
                byte[] pb = ctx.CreatingNewKey ? Create(ctx) : Open(ctx);
                if (pb == null)
                {
                    return null;
                }

                // KeePass clears the returned byte array, thus make a copy
                byte[] pbRet = new byte[pb.Length];
                Array.Copy(pb, pbRet, pb.Length);
                return pbRet;
            }
            catch(Exception ex)
            {
                MessageService.ShowWarning(ex.Message);
            }
            return null;
        }

        private byte[] Create(KeyProviderQueryContext ctx)
        {
            var vaultConf = new SafeVaultConf(ctx.DatabaseIOInfo);

            var vaultConnectionForm = new VaultConnectionConfigForm();
            vaultConnectionForm.InitEx(vaultConf);

            if (UIUtil.ShowDialogAndDestroy(vaultConnectionForm) != DialogResult.OK)
            {
                return null;
            }

            VaultKeyCreateForm createForm = new VaultKeyCreateForm();
            createForm.InitEx(vaultConf, ctx);
            if (UIUtil.ShowDialogAndDestroy(createForm) != DialogResult.OK)
            {
                return null;
            }

            vaultConf.Type = PROVIDER_TYPE;
            vaultConf.Version = PROVIDER_VERSION;

            var masterKey = Encoding.UTF8.GetBytes(vaultConf.DatabaseKeyA);

            var keyLen = (masterKey.Length > 254) ? masterKey.Length : 254;

            var keyA = new byte[keyLen + 2];
            Array.Copy(BitConverter.GetBytes((ushort)masterKey.Length), keyA, 2);
            Array.Copy(masterKey, 0, keyA, 2, masterKey.Length);

            var keyB = Random.Get(keyA.Length);
            for (int i = 0; i < keyB.Length; i++) {
                keyA[i] ^= keyB[i];
            }

            var salt = Random.Get(64);
            using (var aes = new Aes256Cipher())
            {
                aes.SetPassPhrase(salt);
                keyA = aes.Encrypt(keyA);
                keyB = aes.Encrypt(keyB);
            }

            using (var rsa = RsaCipher.LoadFromX509Store(vaultConf.ClientCertificateName))
            {
                salt = rsa.Encrypt(salt);
            }

            vaultConf.Salt = Convert.ToBase64String(salt);
            vaultConf.DatabaseKeyA = Convert.ToBase64String(keyA);
            vaultConf.VaultKeyname = Guid.NewGuid().ToString();
            var databaseKeyB = Convert.ToBase64String(keyB);

            VaultKeyPromptForm promptForm = new VaultKeyPromptForm();
            promptForm.InitEx("Enter SafeVault Password", "Save KeyB to SafeVault", (oneTimePassword) => {

                string status = "";
                var query = new SafeVaultWebClient(vaultConf);
                try
                {
                    status = Async.Invoke(() => query.SetDbxKey(vaultConf.VaultKeyname, databaseKeyB, oneTimePassword));
                    if (status == "OK")
                        return true;

                    MessageService.ShowWarning(
                        query.Utc != null ? "DateTime: " + DateTime.Parse(query.Utc).ToLocalTime() : "",
                        status);
                }
                catch (CryptographicException ex)
                {
                    MessageService.ShowWarning(
                        query.Utc != null ? "DateTime: " + DateTime.Parse(query.Utc).ToLocalTime() : "",
                        ex.Message);
                }
                return false;

            });

            if (UIUtil.ShowDialogAndDestroy(promptForm) != DialogResult.OK)
                return null;

            try
            {
                vaultConf.Save();
            }
            catch(Exception e)
            {
                MessageService.ShowWarning(e.Message);
                return null;
            }

            return masterKey;
        }

        private byte[] Open(KeyProviderQueryContext ctx)
        {
            try
            {
                return OpenInternal(ctx);
            }
            catch (SafeVaultException e)
            {
                MessageService.ShowWarning(e.Message);
                return null;
            }
        }

        private byte[] OpenInternal(KeyProviderQueryContext ctx)
        {
            SafeVaultConf conf = new SafeVaultConf(ctx.DatabaseIOInfo);

            var required = new[] {
                conf.ClientCertificateName,
                conf.ServerUrl,
                conf.ServerCertificateName,
                conf.Salt,
                conf.Username,
                conf.VaultKeyname,
                conf.DatabaseKeyA};

            if (required.Any(string.IsNullOrEmpty))
                throw new ConfigurationException("SafeVault not configured.");

            byte[] salt = Convert.FromBase64String(conf.Salt);
            using (var rsa = RsaCipher.LoadFromX509Store(conf.ClientCertificateName))
            {
                salt = rsa.Decrypt(salt);
            }

            string sKeyB = string.Empty;
            VaultKeyPromptForm promptForm = new VaultKeyPromptForm();
            promptForm.InitEx("Enter SafeVault Password", "Open Database", (oneTimePassword) => {

                var query = new SafeVaultWebClient(conf);
                try
                {
                    sKeyB = query.GetDbxKey(conf.VaultKeyname, oneTimePassword);
                    return true;
                }
                catch (SafeVaultException ex)
                {
                    MessageService.ShowWarning(
                        query.Utc != null ? "DateTime: " + DateTime.Parse(query.Utc).ToLocalTime() : "",
                        ex.Message
                   );
                }
                return false;
            });

            if (UIUtil.ShowDialogAndDestroy(promptForm) != DialogResult.OK)
                return null;

            byte[] keyA = Convert.FromBase64String(conf.DatabaseKeyA);
            byte[] keyB = Convert.FromBase64String(sKeyB);
            using (var aes = new Aes256Cipher())
            {
                aes.SetPassPhrase(salt);
                keyA = aes.Decrypt(keyA);
                keyB = aes.Decrypt(keyB);
            }

            if (keyA.Length != keyB.Length)
                throw new SafevaultKeyProviderException("Incompatible KEYA and KEYB");

            for (int i = 0; i < keyB.Length; i++)
            {
                keyA[i] ^= keyB[i];
            }
            int keyL = BitConverter.ToUInt16(keyA, 0);
            if (keyL > keyA.Length)
                throw new SafevaultKeyProviderException("Invalid KEYB");

            byte[] masterKey = new byte[keyL];
            Array.Copy(keyA, 2, masterKey, 0, masterKey.Length);

            return masterKey;
        }
    }
}
