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
    partial class VaultKeyCreateForm
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
            this.btn_PassCreate = new System.Windows.Forms.Button();
            this.tb_passPhrase2 = new System.Windows.Forms.TextBox();
            this.tb_passPhrase1 = new System.Windows.Forms.TextBox();
            this.lb_Label1 = new System.Windows.Forms.Label();
            this.lb_Label2 = new System.Windows.Forms.Label();
            this.pb_Image1 = new System.Windows.Forms.PictureBox();
            this.lb_PasswordLen = new System.Windows.Forms.Label();
            this.gb_Frame1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pb_Image1)).BeginInit();
            this.SuspendLayout();
            // 
            // btn_OK
            // 
            this.btn_OK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btn_OK.Location = new System.Drawing.Point(208, 341);
            this.btn_OK.Name = "btn_OK";
            this.btn_OK.Size = new System.Drawing.Size(75, 23);
            this.btn_OK.TabIndex = 0;
            this.btn_OK.Text = "&OK";
            this.btn_OK.UseVisualStyleBackColor = true;
            this.btn_OK.Click += new System.EventHandler(this.OnBtnOK);
            // 
            // btn_Cancel
            // 
            this.btn_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btn_Cancel.Location = new System.Drawing.Point(289, 341);
            this.btn_Cancel.Name = "btn_Cancel";
            this.btn_Cancel.Size = new System.Drawing.Size(75, 23);
            this.btn_Cancel.TabIndex = 1;
            this.btn_Cancel.Text = "&Cancel";
            this.btn_Cancel.UseVisualStyleBackColor = true;
            // 
            // gb_Frame1
            // 
            this.gb_Frame1.Controls.Add(this.lb_PasswordLen);
            this.gb_Frame1.Controls.Add(this.btn_PassCreate);
            this.gb_Frame1.Controls.Add(this.tb_passPhrase2);
            this.gb_Frame1.Controls.Add(this.tb_passPhrase1);
            this.gb_Frame1.Controls.Add(this.lb_Label1);
            this.gb_Frame1.Controls.Add(this.lb_Label2);
            this.gb_Frame1.Location = new System.Drawing.Point(12, 66);
            this.gb_Frame1.Name = "gb_Frame1";
            this.gb_Frame1.Size = new System.Drawing.Size(352, 269);
            this.gb_Frame1.TabIndex = 3;
            this.gb_Frame1.TabStop = false;
            this.gb_Frame1.Text = "KeePass Database Password:";
            // 
            // btn_PassCreate
            // 
            this.btn_PassCreate.Location = new System.Drawing.Point(317, 40);
            this.btn_PassCreate.Name = "btn_PassCreate";
            this.btn_PassCreate.Size = new System.Drawing.Size(29, 20);
            this.btn_PassCreate.TabIndex = 14;
            this.btn_PassCreate.Text = "...";
            this.btn_PassCreate.UseVisualStyleBackColor = true;
            this.btn_PassCreate.Click += new System.EventHandler(this.button1_Click);
            // 
            // tb_passPhrase2
            // 
            this.tb_passPhrase2.BackColor = System.Drawing.SystemColors.Window;
            this.tb_passPhrase2.Font = new System.Drawing.Font("MS Reference Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.tb_passPhrase2.Location = new System.Drawing.Point(10, 90);
            this.tb_passPhrase2.Name = "tb_passPhrase2";
            this.tb_passPhrase2.Size = new System.Drawing.Size(301, 21);
            this.tb_passPhrase2.TabIndex = 13;
            this.tb_passPhrase2.Text = "123";
            this.tb_passPhrase2.TextChanged += new System.EventHandler(this.OnPassPhraseChanged);
            // 
            // tb_passPhrase1
            // 
            this.tb_passPhrase1.Font = new System.Drawing.Font("MS Reference Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.tb_passPhrase1.Location = new System.Drawing.Point(9, 41);
            this.tb_passPhrase1.Name = "tb_passPhrase1";
            this.tb_passPhrase1.Size = new System.Drawing.Size(302, 21);
            this.tb_passPhrase1.TabIndex = 1;
            this.tb_passPhrase1.Text = "12345";
            this.tb_passPhrase1.TextChanged += new System.EventHandler(this.OnTextBoxChanged);
            // 
            // lb_Label1
            // 
            this.lb_Label1.AutoSize = true;
            this.lb_Label1.Location = new System.Drawing.Point(6, 25);
            this.lb_Label1.Name = "lb_Label1";
            this.lb_Label1.Size = new System.Drawing.Size(66, 13);
            this.lb_Label1.TabIndex = 12;
            this.lb_Label1.Text = "PassPhrase:";
            // 
            // lb_Label2
            // 
            this.lb_Label2.AutoSize = true;
            this.lb_Label2.Location = new System.Drawing.Point(6, 74);
            this.lb_Label2.Name = "lb_Label2";
            this.lb_Label2.Size = new System.Drawing.Size(104, 13);
            this.lb_Label2.TabIndex = 7;
            this.lb_Label2.Text = "Repeat PassPhrase:";
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
            // lb_PasswordLen
            // 
            this.lb_PasswordLen.Location = new System.Drawing.Point(207, 25);
            this.lb_PasswordLen.Name = "lb_PasswordLen";
            this.lb_PasswordLen.Size = new System.Drawing.Size(104, 13);
            this.lb_PasswordLen.TabIndex = 15;
            this.lb_PasswordLen.Text = "0";
            this.lb_PasswordLen.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // VaultKeyCreateForm
            // 
            this.AcceptButton = this.btn_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btn_Cancel;
            this.ClientSize = new System.Drawing.Size(376, 375);
            this.Controls.Add(this.gb_Frame1);
            this.Controls.Add(this.btn_Cancel);
            this.Controls.Add(this.btn_OK);
            this.Controls.Add(this.pb_Image1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "VaultKeyCreateForm";
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
        private System.Windows.Forms.TextBox tb_passPhrase1;
        private System.Windows.Forms.Label lb_Label1;
        private System.Windows.Forms.Label lb_Label2;
        private System.Windows.Forms.Button btn_PassCreate;
        private System.Windows.Forms.TextBox tb_passPhrase2;
        private System.Windows.Forms.Label lb_PasswordLen;
    }
}