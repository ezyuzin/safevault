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

namespace SafeVault.Forms
{
    public partial class VaultKeyPromptForm : Form
    {
        private Func<string, bool> _okButtonCallback;
        private string _title;
        private string _desc;

        public void InitEx(string title, string desc, Func<string, bool> okButtonCallback)
        {
            _okButtonCallback = okButtonCallback;
            _title = title;
            _desc = desc;
        }

        public VaultKeyPromptForm()
        {
            InitializeComponent();
        }

        private void OnFormLoad(object sender, EventArgs e)
        {
            GlobalWindowManager.AddWindow(this);

            lb_Label2.Text = "";
            this.Text = _desc;
            BannerFactory.CreateBannerEx(this, pb_Image1, Resources.SafeVault.B48x48_SafeVault, _desc, _title);
            this.ActiveControl = tb_oneTimePassword;
        }

        private void OnFormClosed(object sender, FormClosedEventArgs e)
        {
            GlobalWindowManager.RemoveWindow(this);
        }

        public void SetStatusLabelText(string message)
        {
            this.lb_Label2.Text = message;
        }

        private void OnBtnOk(object sender, EventArgs e)
        {
            this.DialogResult = _okButtonCallback.Invoke(tb_oneTimePassword.Text)
                ? DialogResult.OK
                : DialogResult.None;
        }
    }
}
