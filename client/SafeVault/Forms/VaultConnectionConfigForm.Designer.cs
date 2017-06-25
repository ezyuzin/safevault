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

namespace SafeVault.Forms
{
    partial class VaultConnectionConfigForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btn_OK = new System.Windows.Forms.Button();
            this.btn_Cancel = new System.Windows.Forms.Button();
            this.gb_Frame1 = new System.Windows.Forms.GroupBox();
            this.cb_AutoSync = new System.Windows.Forms.ComboBox();
            this.tb_vaultServerUrl = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.lb_Label5 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.cb_serverCertificate = new System.Windows.Forms.ComboBox();
            this.tb_SyncPassword = new System.Windows.Forms.TextBox();
            this.cb_clientCertificate = new System.Windows.Forms.ComboBox();
            this.lb_Label1 = new System.Windows.Forms.Label();
            this.lb_Label2 = new System.Windows.Forms.Label();
            this.lb_Label3 = new System.Windows.Forms.Label();
            this.tb_vaultUsername = new System.Windows.Forms.TextBox();
            this.pb_Image1 = new System.Windows.Forms.PictureBox();
            this.btn_PassCreate = new System.Windows.Forms.Button();
            this.gb_Frame1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pb_Image1)).BeginInit();
            this.SuspendLayout();
            // 
            // btn_OK
            // 
            this.btn_OK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btn_OK.Location = new System.Drawing.Point(311, 277);
            this.btn_OK.Name = "btn_OK";
            this.btn_OK.Size = new System.Drawing.Size(75, 23);
            this.btn_OK.TabIndex = 0;
            this.btn_OK.Text = "&OK";
            this.btn_OK.UseVisualStyleBackColor = true;
            this.btn_OK.Click += new System.EventHandler(this.OnBtnOk);
            // 
            // btn_Cancel
            // 
            this.btn_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btn_Cancel.Location = new System.Drawing.Point(392, 277);
            this.btn_Cancel.Name = "btn_Cancel";
            this.btn_Cancel.Size = new System.Drawing.Size(75, 23);
            this.btn_Cancel.TabIndex = 1;
            this.btn_Cancel.Text = "&Cancel";
            this.btn_Cancel.UseVisualStyleBackColor = true;
            // 
            // gb_Frame1
            // 
            this.gb_Frame1.Controls.Add(this.btn_PassCreate);
            this.gb_Frame1.Controls.Add(this.cb_AutoSync);
            this.gb_Frame1.Controls.Add(this.tb_vaultServerUrl);
            this.gb_Frame1.Controls.Add(this.label1);
            this.gb_Frame1.Controls.Add(this.lb_Label5);
            this.gb_Frame1.Controls.Add(this.label3);
            this.gb_Frame1.Controls.Add(this.cb_serverCertificate);
            this.gb_Frame1.Controls.Add(this.tb_SyncPassword);
            this.gb_Frame1.Controls.Add(this.cb_clientCertificate);
            this.gb_Frame1.Controls.Add(this.lb_Label1);
            this.gb_Frame1.Controls.Add(this.lb_Label2);
            this.gb_Frame1.Controls.Add(this.lb_Label3);
            this.gb_Frame1.Controls.Add(this.tb_vaultUsername);
            this.gb_Frame1.Location = new System.Drawing.Point(12, 66);
            this.gb_Frame1.Name = "gb_Frame1";
            this.gb_Frame1.Size = new System.Drawing.Size(461, 205);
            this.gb_Frame1.TabIndex = 3;
            this.gb_Frame1.TabStop = false;
            this.gb_Frame1.Text = "SafeVault";
            // 
            // cb_AutoSync
            // 
            this.cb_AutoSync.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb_AutoSync.FormattingEnabled = true;
            this.cb_AutoSync.Items.AddRange(new object[] {
            "Disabled",
            "Save",
            "Open",
            "Both"});
            this.cb_AutoSync.Location = new System.Drawing.Point(124, 168);
            this.cb_AutoSync.Name = "cb_AutoSync";
            this.cb_AutoSync.Size = new System.Drawing.Size(110, 21);
            this.cb_AutoSync.TabIndex = 15;
            this.cb_AutoSync.SelectedIndexChanged += new System.EventHandler(this.OnCtrSelectedIndexChanged);
            // 
            // tb_vaultServerUrl
            // 
            this.tb_vaultServerUrl.Location = new System.Drawing.Point(124, 25);
            this.tb_vaultServerUrl.Name = "tb_vaultServerUrl";
            this.tb_vaultServerUrl.Size = new System.Drawing.Size(331, 20);
            this.tb_vaultServerUrl.TabIndex = 1;
            this.tb_vaultServerUrl.TextChanged += new System.EventHandler(this.OnTextBoxChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 171);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 13);
            this.label1.TabIndex = 14;
            this.label1.Text = "Auto Sync:";
            // 
            // lb_Label5
            // 
            this.lb_Label5.AutoSize = true;
            this.lb_Label5.Location = new System.Drawing.Point(9, 28);
            this.lb_Label5.Name = "lb_Label5";
            this.lb_Label5.Size = new System.Drawing.Size(57, 13);
            this.lb_Label5.TabIndex = 12;
            this.lb_Label5.Text = "Server Url:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 145);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(83, 13);
            this.label3.TabIndex = 12;
            this.label3.Text = "Sync Password:";
            // 
            // cb_serverCertificate
            // 
            this.cb_serverCertificate.FormattingEnabled = true;
            this.cb_serverCertificate.Location = new System.Drawing.Point(124, 51);
            this.cb_serverCertificate.Name = "cb_serverCertificate";
            this.cb_serverCertificate.Size = new System.Drawing.Size(331, 21);
            this.cb_serverCertificate.TabIndex = 2;
            this.cb_serverCertificate.SelectedIndexChanged += new System.EventHandler(this.OnCtrSelectedIndexChanged);
            this.cb_serverCertificate.SelectionChangeCommitted += new System.EventHandler(this.OnCtrSelectedIndexChanged);
            // 
            // tb_SyncPassword
            // 
            this.tb_SyncPassword.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.tb_SyncPassword.Location = new System.Drawing.Point(124, 142);
            this.tb_SyncPassword.Name = "tb_SyncPassword";
            this.tb_SyncPassword.PasswordChar = '•';
            this.tb_SyncPassword.Size = new System.Drawing.Size(299, 21);
            this.tb_SyncPassword.TabIndex = 13;
            this.tb_SyncPassword.TextChanged += new System.EventHandler(this.OnTextBoxChanged);
            // 
            // cb_clientCertificate
            // 
            this.cb_clientCertificate.FormattingEnabled = true;
            this.cb_clientCertificate.Location = new System.Drawing.Point(124, 115);
            this.cb_clientCertificate.Name = "cb_clientCertificate";
            this.cb_clientCertificate.Size = new System.Drawing.Size(331, 21);
            this.cb_clientCertificate.TabIndex = 5;
            this.cb_clientCertificate.SelectedIndexChanged += new System.EventHandler(this.OnCtrSelectedIndexChanged);
            this.cb_clientCertificate.SelectionChangeCommitted += new System.EventHandler(this.OnCtrSelectedIndexChanged);
            // 
            // lb_Label1
            // 
            this.lb_Label1.AutoSize = true;
            this.lb_Label1.Location = new System.Drawing.Point(9, 118);
            this.lb_Label1.Name = "lb_Label1";
            this.lb_Label1.Size = new System.Drawing.Size(86, 13);
            this.lb_Label1.TabIndex = 9;
            this.lb_Label1.Text = "Client Certificate:";
            // 
            // lb_Label2
            // 
            this.lb_Label2.AutoSize = true;
            this.lb_Label2.Location = new System.Drawing.Point(9, 54);
            this.lb_Label2.Name = "lb_Label2";
            this.lb_Label2.Size = new System.Drawing.Size(91, 13);
            this.lb_Label2.TabIndex = 7;
            this.lb_Label2.Text = "Server Certificate:";
            // 
            // lb_Label3
            // 
            this.lb_Label3.AutoSize = true;
            this.lb_Label3.Location = new System.Drawing.Point(9, 92);
            this.lb_Label3.Name = "lb_Label3";
            this.lb_Label3.Size = new System.Drawing.Size(58, 13);
            this.lb_Label3.TabIndex = 5;
            this.lb_Label3.Text = "Username:";
            // 
            // tb_vaultUsername
            // 
            this.tb_vaultUsername.Location = new System.Drawing.Point(124, 89);
            this.tb_vaultUsername.Name = "tb_vaultUsername";
            this.tb_vaultUsername.Size = new System.Drawing.Size(331, 20);
            this.tb_vaultUsername.TabIndex = 3;
            this.tb_vaultUsername.TextChanged += new System.EventHandler(this.OnTextBoxChanged);
            // 
            // pb_Image1
            // 
            this.pb_Image1.Dock = System.Windows.Forms.DockStyle.Top;
            this.pb_Image1.Location = new System.Drawing.Point(0, 0);
            this.pb_Image1.Name = "pb_Image1";
            this.pb_Image1.Size = new System.Drawing.Size(482, 60);
            this.pb_Image1.TabIndex = 0;
            this.pb_Image1.TabStop = false;
            // 
            // btn_PassCreate
            // 
            this.btn_PassCreate.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btn_PassCreate.Location = new System.Drawing.Point(426, 143);
            this.btn_PassCreate.Margin = new System.Windows.Forms.Padding(0);
            this.btn_PassCreate.Name = "btn_PassCreate";
            this.btn_PassCreate.Size = new System.Drawing.Size(29, 20);
            this.btn_PassCreate.TabIndex = 16;
            this.btn_PassCreate.Text = "•••";
            this.btn_PassCreate.UseVisualStyleBackColor = true;
            this.btn_PassCreate.Click += new System.EventHandler(this.btn_PassCreate_Click);
            // 
            // VaultConnectionConfigForm
            // 
            this.AcceptButton = this.btn_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btn_Cancel;
            this.ClientSize = new System.Drawing.Size(482, 312);
            this.Controls.Add(this.gb_Frame1);
            this.Controls.Add(this.btn_Cancel);
            this.Controls.Add(this.btn_OK);
            this.Controls.Add(this.pb_Image1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "VaultConnectionConfigForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "<>";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OnFormClosed);
            this.Load += new System.EventHandler(this.OnFormLoad);
            this.gb_Frame1.ResumeLayout(false);
            this.gb_Frame1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pb_Image1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pb_Image1;
        private System.Windows.Forms.Button btn_OK;
        private System.Windows.Forms.Button btn_Cancel;
        private System.Windows.Forms.GroupBox gb_Frame1;
        private System.Windows.Forms.TextBox tb_vaultServerUrl;
        private System.Windows.Forms.Label lb_Label5;
        private System.Windows.Forms.ComboBox cb_serverCertificate;
        private System.Windows.Forms.ComboBox cb_clientCertificate;
        private System.Windows.Forms.Label lb_Label1;
        private System.Windows.Forms.Label lb_Label2;
        private System.Windows.Forms.Label lb_Label3;
        private System.Windows.Forms.TextBox tb_vaultUsername;
        private System.Windows.Forms.ComboBox cb_AutoSync;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tb_SyncPassword;
        private System.Windows.Forms.Button btn_PassCreate;
    }
}