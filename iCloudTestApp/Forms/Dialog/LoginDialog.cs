using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using iCloudLib;

namespace iCloudTestApp.Forms.Dialog
{
    public partial class LoginDialog : Form
    {
        private iCloud client;
        public LoginDialog(iCloud clientConnection)
        {
            InitializeComponent();
            this.client = clientConnection;

            // Quick Debug Login Information.
            #if DEBUG
            string debugAuthPath = "C:\\iCloudDebugAuth.txt";
            if (File.Exists(debugAuthPath))
            {
                string authData = System.Text.ASCIIEncoding.ASCII.GetString(File.ReadAllBytes(debugAuthPath));
                this.txtAppleId.Text = authData.Substring(0, authData.IndexOf(':'));
                this.txtPassword.Text = authData.Substring(authData.IndexOf(':') + 1);
            }
            #endif
        }

        private void btnAuthenticate_Click(object sender, EventArgs e)
        {
            // Attempt our connection with our client.
            client.Connect(new iCloud.iCloudLoginCredentials(txtAppleId.Text, txtPassword.Text, chkExtendedLogin.Checked));

            // If we're connected, close the window.
            if (client.IsConnected)
            {
                this.Close();
            }
            else
                // We couldn't connect
                MessageBox.Show("Failed to authenticate.");
        }

        private void txtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            // We want the enter key to authenticate on the text boxes as well.
            if (e.KeyCode == Keys.Enter)
                btnAuthenticate_Click(null, null);
        }
    }
}
