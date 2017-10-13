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
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;
using KeePass.UI;
using KeePassLib.Security;
using SafeVault.Configuration;

namespace SafeVault.Forms
{
    internal partial class VaultConnectionConfigForm : Form
    {
        private const string NO_PASSWORD = @"(no password)";
        private SafeVaultConf _vaultConf = null;

        public void InitEx(SafeVaultConf vaultConf)
        {
            _vaultConf = vaultConf;
        }

        public VaultConnectionConfigForm()
        {
            InitializeComponent();
        }

        private void OnFormLoad(object sender, EventArgs e)
        {
            if (_vaultConf == null)
            {
                throw new InvalidOperationException();
            }

            GlobalWindowManager.AddWindow(this);

            string strTitle = "Configure SafeVault Connection";
            string strDesc = "Protect the database using an OTP token.";

            this.Text = strTitle;
            BannerFactory.CreateBannerEx(this, pb_Image1, Resources.SafeVault.B48x48_SafeVault, strTitle, strDesc);

            tb_vaultUsername.Text = _vaultConf.Username;
            tb_vaultServerUrl.Text = _vaultConf.ServerUrl;
            if (string.IsNullOrEmpty(tb_vaultServerUrl.Text))
            {
                tb_vaultServerUrl.Text = "https://www.safevault.com/api/get.cgi";
            }

            var store = new X509Store(StoreName.My);
            store.Open(OpenFlags.ReadOnly);
            foreach (var x509 in store.Certificates)
            {
                var cn = x509.FriendlyName;
                if (string.IsNullOrEmpty(cn))
                    continue;

                cb_clientCertificate.Items.Add(cn);
                cb_serverCertificate.Items.Add(cn);
            }
            if (!string.IsNullOrEmpty(_vaultConf.ClientCertificateName))
            {
                cb_clientCertificate.SelectedIndex = cb_clientCertificate.Items.IndexOf(_vaultConf.ClientCertificateName);
            }

            if (!string.IsNullOrEmpty(_vaultConf.ServerCertificateName))
            {
                cb_serverCertificate.SelectedIndex = cb_serverCertificate.Items.IndexOf(_vaultConf.ServerCertificateName);
            }

            cb_AutoSync.Enabled = true;
            cb_AutoSync.Items.Clear();
            foreach(var value in Enum.GetValues(typeof(AutoSyncMode)))
                cb_AutoSync.Items.Add(value);

            cb_AutoSync.SelectedIndex = Enum.GetValues(typeof(AutoSyncMode))
                .Cast<AutoSyncMode>().ToList()
                .IndexOf(_vaultConf.AutoSyncMode);

            tb_SyncPassword.PasswordChar = '•';
            if (!string.IsNullOrEmpty(_vaultConf.SyncPassword?.ReadString()))
                tb_SyncPassword.Text = NO_PASSWORD;

            EnableControlsEx();
        }

        private void OnFormClosed(object sender, FormClosedEventArgs e)
        {
            GlobalWindowManager.RemoveWindow(this);
        }

        private void OnBtnOk(object sender, EventArgs e)
        {
            _vaultConf.Username = tb_vaultUsername.Text;
            _vaultConf.ServerUrl = tb_vaultServerUrl.Text;

            _vaultConf.ClientCertificateName = (string)cb_clientCertificate.Items[cb_clientCertificate.SelectedIndex];
            _vaultConf.ServerCertificateName = (string)cb_serverCertificate.Items[cb_serverCertificate.SelectedIndex];

            _vaultConf.AutoSyncMode = (AutoSyncMode)cb_AutoSync.SelectedItem;

            if (tb_SyncPassword.Text != NO_PASSWORD)
                _vaultConf.SyncPassword = new ProtectedString(true, tb_SyncPassword.Text);
            
            this.DialogResult = DialogResult.OK;
        }

        private void EnableControlsEx()
        {
            bool bOk = (!string.IsNullOrEmpty(tb_vaultUsername.Text));
            bOk &= (!string.IsNullOrEmpty(tb_vaultServerUrl.Text));
            bOk &= (cb_clientCertificate.SelectedIndex != -1);
            bOk &= (cb_serverCertificate.SelectedIndex != -1);
            bOk &= (!string.IsNullOrEmpty(tb_SyncPassword.Text));

            btn_OK.Enabled = bOk;
        }

        private void OnCtrSelectedIndexChanged(object sender, EventArgs e)
        {
            EnableControlsEx();
        }

        private void OnTextBoxChanged(object sender, EventArgs e)
        {
            EnableControlsEx();
        }

        private void btn_PassCreate_Click(object sender, EventArgs e)
        {
            if (tb_SyncPassword.PasswordChar == '\0')
            {
                tb_SyncPassword.PasswordChar = '•';
            }
            else
            {
                tb_SyncPassword.PasswordChar = '\0';
                if (tb_SyncPassword.Text == NO_PASSWORD)
                    tb_SyncPassword.Text = _vaultConf.SyncPassword.ReadString();
            }

        }
    }
}
