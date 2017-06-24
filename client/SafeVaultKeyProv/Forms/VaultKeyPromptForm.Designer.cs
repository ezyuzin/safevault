namespace SafeVaultKeyPlugin.Forms
{
    partial class VaultKeyPromptForm
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
            if(disposing && (components != null))
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
            this.pb_Image1 = new System.Windows.Forms.PictureBox();
            this.tb_oneTimePassword = new System.Windows.Forms.TextBox();
            this.lb_Label1 = new System.Windows.Forms.Label();
            this.lb_Label2 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pb_Image1)).BeginInit();
            this.SuspendLayout();
            // 
            // m_btnOK
            // 
            this.btn_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_OK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btn_OK.Location = new System.Drawing.Point(121, 145);
            this.btn_OK.Name = "m_btnOK";
            this.btn_OK.Size = new System.Drawing.Size(75, 23);
            this.btn_OK.TabIndex = 1;
            this.btn_OK.Text = "&OK";
            this.btn_OK.UseVisualStyleBackColor = true;
            this.btn_OK.Click += new System.EventHandler(this.OnBtnOk);
            // 
            // m_btnCancel
            // 
            this.btn_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btn_Cancel.Location = new System.Drawing.Point(202, 145);
            this.btn_Cancel.Name = "m_btnCancel";
            this.btn_Cancel.Size = new System.Drawing.Size(75, 23);
            this.btn_Cancel.TabIndex = 2;
            this.btn_Cancel.Text = "&Cancel";
            this.btn_Cancel.UseVisualStyleBackColor = true;
            // 
            // m_bannerImage
            // 
            this.pb_Image1.Dock = System.Windows.Forms.DockStyle.Top;
            this.pb_Image1.Location = new System.Drawing.Point(0, 0);
            this.pb_Image1.Name = "m_bannerImage";
            this.pb_Image1.Size = new System.Drawing.Size(289, 60);
            this.pb_Image1.TabIndex = 2;
            this.pb_Image1.TabStop = false;
            // 
            // tb_oneTimePassword
            // 
            this.tb_oneTimePassword.Location = new System.Drawing.Point(15, 91);
            this.tb_oneTimePassword.Name = "tb_oneTimePassword";
            this.tb_oneTimePassword.Size = new System.Drawing.Size(262, 20);
            this.tb_oneTimePassword.TabIndex = 7;
            // 
            // m_lblEnterOtps
            // 
            this.lb_Label1.AutoSize = true;
            this.lb_Label1.Location = new System.Drawing.Point(12, 75);
            this.lb_Label1.Name = "m_lblEnterOtps";
            this.lb_Label1.Size = new System.Drawing.Size(126, 13);
            this.lb_Label1.TabIndex = 8;
            this.lb_Label1.Text = "Enter one-time password:";
            // 
            // label1
            // 
            this.lb_Label2.AutoSize = true;
            this.lb_Label2.Location = new System.Drawing.Point(15, 118);
            this.lb_Label2.Name = "label1";
            this.lb_Label2.Size = new System.Drawing.Size(35, 13);
            this.lb_Label2.TabIndex = 9;
            this.lb_Label2.Text = "(status text)";
            // 
            // VaultKeyPromptForm
            // 
            this.AcceptButton = this.btn_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btn_Cancel;
            this.ClientSize = new System.Drawing.Size(289, 176);
            this.Controls.Add(this.lb_Label2);
            this.Controls.Add(this.tb_oneTimePassword);
            this.Controls.Add(this.lb_Label1);
            this.Controls.Add(this.pb_Image1);
            this.Controls.Add(this.btn_Cancel);
            this.Controls.Add(this.btn_OK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "VaultKeyPromptForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "<>";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OnFormClosed);
            this.Load += new System.EventHandler(this.OnFormLoad);
            ((System.ComponentModel.ISupportInitialize)(this.pb_Image1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btn_OK;
        private System.Windows.Forms.Button btn_Cancel;
        private System.Windows.Forms.PictureBox pb_Image1;
        private System.Windows.Forms.TextBox tb_oneTimePassword;
        private System.Windows.Forms.Label lb_Label1;
        private System.Windows.Forms.Label lb_Label2;
    }
}