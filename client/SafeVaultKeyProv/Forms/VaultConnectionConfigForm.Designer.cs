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

namespace SafeVaultKeyPlugin.Forms
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
            this.tb_vaultServerUrl = new System.Windows.Forms.TextBox();
            this.lb_Label5 = new System.Windows.Forms.Label();
            this.cb_serverCertificate = new System.Windows.Forms.ComboBox();
            this.cb_clientCertificate = new System.Windows.Forms.ComboBox();
            this.lb_Label1 = new System.Windows.Forms.Label();
            this.lb_Label2 = new System.Windows.Forms.Label();
            this.lb_Label3 = new System.Windows.Forms.Label();
            this.tb_vaultUsername = new System.Windows.Forms.TextBox();
            this.pb_Image1 = new System.Windows.Forms.PictureBox();
            this.gb_Frame1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pb_Image1)).BeginInit();
            this.SuspendLayout();
            // 
            // btn_OK
            // 
            this.btn_OK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btn_OK.Location = new System.Drawing.Point(208, 288);
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
            this.btn_Cancel.Location = new System.Drawing.Point(289, 288);
            this.btn_Cancel.Name = "btn_Cancel";
            this.btn_Cancel.Size = new System.Drawing.Size(75, 23);
            this.btn_Cancel.TabIndex = 1;
            this.btn_Cancel.Text = "&Cancel";
            this.btn_Cancel.UseVisualStyleBackColor = true;
            // 
            // gb_Frame1
            // 
            this.gb_Frame1.Controls.Add(this.tb_vaultServerUrl);
            this.gb_Frame1.Controls.Add(this.lb_Label5);
            this.gb_Frame1.Controls.Add(this.cb_serverCertificate);
            this.gb_Frame1.Controls.Add(this.cb_clientCertificate);
            this.gb_Frame1.Controls.Add(this.lb_Label1);
            this.gb_Frame1.Controls.Add(this.lb_Label2);
            this.gb_Frame1.Controls.Add(this.lb_Label3);
            this.gb_Frame1.Controls.Add(this.tb_vaultUsername);
            this.gb_Frame1.Location = new System.Drawing.Point(12, 66);
            this.gb_Frame1.Name = "gb_Frame1";
            this.gb_Frame1.Size = new System.Drawing.Size(352, 216);
            this.gb_Frame1.TabIndex = 3;
            this.gb_Frame1.TabStop = false;
            this.gb_Frame1.Text = "Vault";
            // 
            // tb_vaultServerUrl
            // 
            this.tb_vaultServerUrl.Location = new System.Drawing.Point(9, 41);
            this.tb_vaultServerUrl.Name = "tb_vaultServerUrl";
            this.tb_vaultServerUrl.Size = new System.Drawing.Size(337, 20);
            this.tb_vaultServerUrl.TabIndex = 1;
            this.tb_vaultServerUrl.TextChanged += new System.EventHandler(this.OnTextBoxChanged);
            // 
            // lb_Label5
            // 
            this.lb_Label5.AutoSize = true;
            this.lb_Label5.Location = new System.Drawing.Point(6, 25);
            this.lb_Label5.Name = "lb_Label5";
            this.lb_Label5.Size = new System.Drawing.Size(57, 13);
            this.lb_Label5.TabIndex = 12;
            this.lb_Label5.Text = "Server Url:";
            // 
            // cb_serverCertificate
            // 
            this.cb_serverCertificate.FormattingEnabled = true;
            this.cb_serverCertificate.Location = new System.Drawing.Point(8, 80);
            this.cb_serverCertificate.Name = "cb_serverCertificate";
            this.cb_serverCertificate.Size = new System.Drawing.Size(338, 21);
            this.cb_serverCertificate.TabIndex = 2;
            this.cb_serverCertificate.SelectedIndexChanged += new System.EventHandler(this.OnCtrSelectedIndexChanged);
            this.cb_serverCertificate.SelectionChangeCommitted += new System.EventHandler(this.OnCtrSelectedIndexChanged);
            // 
            // cb_clientCertificate
            // 
            this.cb_clientCertificate.FormattingEnabled = true;
            this.cb_clientCertificate.Location = new System.Drawing.Point(8, 173);
            this.cb_clientCertificate.Name = "cb_clientCertificate";
            this.cb_clientCertificate.Size = new System.Drawing.Size(337, 21);
            this.cb_clientCertificate.TabIndex = 5;
            this.cb_clientCertificate.SelectedIndexChanged += new System.EventHandler(this.OnCtrSelectedIndexChanged);
            this.cb_clientCertificate.SelectionChangeCommitted += new System.EventHandler(this.OnCtrSelectedIndexChanged);
            // 
            // lb_Label1
            // 
            this.lb_Label1.AutoSize = true;
            this.lb_Label1.Location = new System.Drawing.Point(5, 157);
            this.lb_Label1.Name = "lb_Label1";
            this.lb_Label1.Size = new System.Drawing.Size(122, 13);
            this.lb_Label1.TabIndex = 9;
            this.lb_Label1.Text = "Client Private Certificate:";
            // 
            // lb_Label2
            // 
            this.lb_Label2.AutoSize = true;
            this.lb_Label2.Location = new System.Drawing.Point(5, 64);
            this.lb_Label2.Name = "lb_Label2";
            this.lb_Label2.Size = new System.Drawing.Size(123, 13);
            this.lb_Label2.TabIndex = 7;
            this.lb_Label2.Text = "Server Public Certificate:";
            // 
            // lb_Label3
            // 
            this.lb_Label3.AutoSize = true;
            this.lb_Label3.Location = new System.Drawing.Point(6, 118);
            this.lb_Label3.Name = "lb_Label3";
            this.lb_Label3.Size = new System.Drawing.Size(85, 13);
            this.lb_Label3.TabIndex = 5;
            this.lb_Label3.Text = "Vault Username:";
            // 
            // tb_vaultUsername
            // 
            this.tb_vaultUsername.Location = new System.Drawing.Point(9, 134);
            this.tb_vaultUsername.Name = "tb_vaultUsername";
            this.tb_vaultUsername.Size = new System.Drawing.Size(337, 20);
            this.tb_vaultUsername.TabIndex = 3;
            this.tb_vaultUsername.TextChanged += new System.EventHandler(this.OnTextBoxChanged);
            // 
            // pb_Image1
            // 
            this.pb_Image1.Dock = System.Windows.Forms.DockStyle.Top;
            this.pb_Image1.Location = new System.Drawing.Point(0, 0);
            this.pb_Image1.Name = "pb_Image1";
            this.pb_Image1.Size = new System.Drawing.Size(376, 60);
            this.pb_Image1.TabIndex = 0;
            this.pb_Image1.TabStop = false;
            // 
            // VaultConnectionConfigForm
            // 
            this.AcceptButton = this.btn_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btn_Cancel;
            this.ClientSize = new System.Drawing.Size(376, 326);
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
    }
}