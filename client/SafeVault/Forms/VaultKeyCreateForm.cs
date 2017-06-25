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
using System.Windows.Forms;
using KeePass.UI;
using KeePassLib.Keys;
using SafeVault.Configuration;

namespace SafeVault.Forms
{
    internal partial class VaultKeyCreateForm : Form
    {
        private SafeVaultConf _vaultConf;
        private bool _showPassword;

        public void InitEx(SafeVaultConf vaultConf, KeyProviderQueryContext ctx)
        {
            _vaultConf = vaultConf;
        }

        public VaultKeyCreateForm()
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

            string strTitle = "Create Database Key";
            string strDesc = "Protect the database using an OTP token.";

            tb_passPhrase1.Text = "";
            tb_passPhrase2.Text = "";

            this.Text = strTitle;
            BannerFactory.CreateBannerEx(this, pb_Image1, Resources.SafeVault.B48x48_SafeVault, strTitle, strDesc);

            EnableControlsEx();
        }

        private void OnFormClosed(object sender, FormClosedEventArgs e)
        {
            GlobalWindowManager.RemoveWindow(this);
        }

        private void OnBtnOK(object sender, EventArgs e)
        {
            _vaultConf.DatabaseKeyA = tb_passPhrase1.Text;
            this.DialogResult = DialogResult.OK;
        }

        private void EnableControlsEx()
        {
            tb_passPhrase1.PasswordChar = !_showPassword ? '*' : '\0';
            tb_passPhrase1.UseSystemPasswordChar = !_showPassword;
            tb_passPhrase2.PasswordChar = !_showPassword ? '*' : '\0';
            tb_passPhrase2.UseSystemPasswordChar = !_showPassword;
            tb_passPhrase2.Enabled = !_showPassword;
            tb_passPhrase2.BackColor = _showPassword ? System.Drawing.SystemColors.ButtonShadow : System.Drawing.SystemColors.Window;

            bool bOK = true;
            bOK = (bOK && tb_passPhrase1.Text.Length >= 32);
            bOK &= (bOK && (_showPassword || tb_passPhrase1.Text == tb_passPhrase2.Text));
            btn_OK.Enabled = bOK;

            if (!_showPassword)
            {
                tb_passPhrase2.BackColor = (tb_passPhrase1.Text != tb_passPhrase2.Text)
                ? System.Drawing.Color.Orange
                : System.Drawing.SystemColors.Window;
            }
        }

        private void OnTextBoxChanged(object sender, EventArgs e)
        {
            EnableControlsEx();
            lb_PasswordLen.Text = tb_passPhrase1.Text.Length.ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _showPassword = !_showPassword;
            EnableControlsEx();
        }

        private void OnPassPhraseChanged(object sender, EventArgs e)
        {
            EnableControlsEx();
        }
    }
}
