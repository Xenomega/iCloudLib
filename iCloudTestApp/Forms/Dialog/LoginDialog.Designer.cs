namespace iCloudTestApp.Forms.Dialog
{
    partial class LoginDialog
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
            this.lblAppleID = new System.Windows.Forms.Label();
            this.lblPassword = new System.Windows.Forms.Label();
            this.chkExtendedLogin = new System.Windows.Forms.CheckBox();
            this.btnAuthenticate = new System.Windows.Forms.Button();
            this.txtAppleId = new System.Windows.Forms.TextBox();
            this.txtPassword = new System.Windows.Forms.MaskedTextBox();
            this.SuspendLayout();
            // 
            // lblAppleID
            // 
            this.lblAppleID.AutoSize = true;
            this.lblAppleID.Location = new System.Drawing.Point(9, 9);
            this.lblAppleID.Name = "lblAppleID";
            this.lblAppleID.Size = new System.Drawing.Size(51, 13);
            this.lblAppleID.TabIndex = 0;
            this.lblAppleID.Text = "Apple ID:";
            // 
            // lblPassword
            // 
            this.lblPassword.AutoSize = true;
            this.lblPassword.Location = new System.Drawing.Point(9, 48);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(56, 13);
            this.lblPassword.TabIndex = 1;
            this.lblPassword.Text = "Password:";
            // 
            // chkExtendedLogin
            // 
            this.chkExtendedLogin.AutoSize = true;
            this.chkExtendedLogin.Location = new System.Drawing.Point(12, 90);
            this.chkExtendedLogin.Name = "chkExtendedLogin";
            this.chkExtendedLogin.Size = new System.Drawing.Size(100, 17);
            this.chkExtendedLogin.TabIndex = 2;
            this.chkExtendedLogin.Text = "Extended Login";
            this.chkExtendedLogin.UseVisualStyleBackColor = true;
            // 
            // btnAuthenticate
            // 
            this.btnAuthenticate.Location = new System.Drawing.Point(12, 113);
            this.btnAuthenticate.Name = "btnAuthenticate";
            this.btnAuthenticate.Size = new System.Drawing.Size(260, 23);
            this.btnAuthenticate.TabIndex = 3;
            this.btnAuthenticate.Text = "Authenticate";
            this.btnAuthenticate.UseVisualStyleBackColor = true;
            this.btnAuthenticate.Click += new System.EventHandler(this.btnAuthenticate_Click);
            // 
            // txtAppleId
            // 
            this.txtAppleId.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtAppleId.Location = new System.Drawing.Point(12, 25);
            this.txtAppleId.Name = "txtAppleId";
            this.txtAppleId.Size = new System.Drawing.Size(260, 20);
            this.txtAppleId.TabIndex = 4;
            this.txtAppleId.Text = "example@gmail.com";
            this.txtAppleId.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtPassword_KeyDown);
            // 
            // txtPassword
            // 
            this.txtPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPassword.Location = new System.Drawing.Point(12, 64);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.PasswordChar = '*';
            this.txtPassword.Size = new System.Drawing.Size(260, 20);
            this.txtPassword.TabIndex = 5;
            this.txtPassword.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtPassword_KeyDown);
            // 
            // LoginDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 146);
            this.Controls.Add(this.txtPassword);
            this.Controls.Add(this.txtAppleId);
            this.Controls.Add(this.btnAuthenticate);
            this.Controls.Add(this.chkExtendedLogin);
            this.Controls.Add(this.lblPassword);
            this.Controls.Add(this.lblAppleID);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.Name = "LoginDialog";
            this.Text = "iCloud Login";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblAppleID;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.CheckBox chkExtendedLogin;
        private System.Windows.Forms.Button btnAuthenticate;
        private System.Windows.Forms.TextBox txtAppleId;
        private System.Windows.Forms.MaskedTextBox txtPassword;
    }
}