using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using iCloudLib;
using iCloudTestApp.Forms.Dialog;

namespace iCloudTestApp
{
    public partial class Form1 : Form
    {
        private iCloud client;
        /// <summary>
        /// Our client we'll be using to connect to iCloud servers.
        /// </summary>
        public iCloud Client
        {
            get
            {
                return client;
            }
        }

        public Form1()
        {
            InitializeComponent();

            // Initialize our client.
            client = new iCloudLib.iCloud();
            Client.FindMyiPhoneUpdate += FindMyPhoneUpdated;
            LoadContactInformation();
        }

        private void ShowLoginWindow()
        {
            LoginDialog loginDialog = new LoginDialog(Client);
            loginDialog.ShowDialog();
            if (Client.IsConnected)
            {
                startTime = 0;
                Client.StartFindMyiPhone(1000);
                //TestContactFunctions();
                LoadContactInformation();
                btnLogin.Enabled = false;
                button1.Enabled = true;
                button2.Enabled = true;
            }
        }
        private void TestContactFunctions()
        {
            // Test contacts functions..
            // Add a John iCloudSeed contact, edit all existing John iCloudSeeds, then delete them all.

            // Add a contact.
            iCloud.iCloudContactsContact newContact = new iCloud.iCloudContactsContact();
            newContact.FirstName = "John";
            newContact.LastName = "iCloudSeed";
            Client.SetContact(newContact);

            // Edit contacts
            iCloud.iCloudContactsContact[] edittedContacts = Client.Contacts.Where(x => x.FirstName == "John" && x.LastName == "iCloudSeed").ToArray();
            foreach (iCloud.iCloudContactsContact editContact in edittedContacts)
                editContact.MiddleName = "TestEdit";
            Client.SetContacts(edittedContacts);

            // Remove contacts
            if (edittedContacts.Length > 0)
                Client.DeleteContacts(edittedContacts);
        }
        private void LoadContactInformation()
        {
            listContacts.Items.Clear();
            if (Client.IsConnected)
                Client.GetContacts();
            else
                return;
            if (Client.Contacts != null)
                foreach (iCloud.iCloudContactsContact contact in Client.Contacts)
                {
                    // Determine our contact display name for this application.
                    string displayName = string.Format("{0} {1} {2}", contact.FirstName, contact.MiddleName, contact.LastName).Trim();
                    if (string.IsNullOrEmpty(displayName))
                        displayName = string.Format("{0} {1}", contact.PhoneticFirstName, contact.PhoneticLastName).Trim();
                    if (string.IsNullOrEmpty(displayName))
                        displayName = contact.NickName;
                    if (string.IsNullOrEmpty(displayName))
                        displayName = contact.CompanyName;
                    displayName = displayName.Replace("  ", " ");
                    if (string.IsNullOrEmpty(displayName))
                        displayName = "n/a";

                    // Create our listview item.
                    ListViewItem lvi = new ListViewItem(displayName);
                    lvi.Tag = contact;
                    listContacts.Items.Add(lvi);
                }
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            ShowLoginWindow();
        }
        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            listContacts.Items.Clear();
            txtContactInfo.Text = "";
            txtOutput.Text = "";
            Client.StopFindMyiPhone();
            Client.Disconnect();
            btnLogin.Enabled = true;
            button1.Enabled = false;
            button2.Enabled = false;
        }
        private void btnPlaySound_Click(object sender, EventArgs e)
        {
            if (Client.Devices != null)
                foreach (iCloud.iCloudDevice device in Client.Devices)
                    if (device.DeviceClass == "iPhone")
                        Client.PlaySound("Test iPhone Only Alert", device);
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Environment.Exit(0);
        }

        ulong startTime = 0;
        private void FindMyPhoneUpdated(iCloud.iCloudFMIClientResponse response)
        {
            // Invoke our cross thread UI update if required.
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)(() => FindMyPhoneUpdated(response)));
                return;
            }

            // Update the textbox.
            if (startTime == 0)
                startTime = response.ServerContext.ServerTimeStamp;
            ulong secondsElapsed = (response.ServerContext.ServerTimeStamp - startTime) / 1000;
            txtOutput.Text = string.Format("Time Elapsed: {0}s\n", secondsElapsed);
            foreach(iCloud.iCloudDevice d in response.Devices)
                txtOutput.Text += string.Format(
                 @"
Device Name: {0}
Device: {1}
Battery: {2}%
Battery Status: {3}
Longitude: {4}
Latitude: {5}
IsLocating: {6}
",
  d.DeviceName,
  d.DeviceDisplayName,
(d.BatteryLevel * 100).ToString("n2"),
d.BatteryStatus,
d.Location != null ? d.Location.Longitude.ToString() : "n/a",
d.Location != null ? d.Location.Latitude.ToString() : "n/a",
d.IsLocating
 );
            Text = string.Format("Welcome to iCloudTest {0}!", Client.UserInformation.FirstName);
        }

        private void listContacts_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Make sure we have a contact selected
            if (listContacts.SelectedItems.Count == 0)
                return;

            // Obtain our contact
            iCloud.iCloudContactsContact contact = (iCloud.iCloudContactsContact)listContacts.SelectedItems[0].Tag;

            // Format our dates section text
            string strDates = "";
            if(contact.Dates != null)
                foreach (iCloud.iCloudContactsStringEntry entry in contact.Dates)
                    strDates += string.Format("{0}: {1}\r\n", entry.Label.ToUpper(), entry.Field);
            strDates = strDates.Trim();

            // Format our phones section text
            string strPhones = "";
            if(contact.Phones != null)
                foreach(iCloud.iCloudContactsStringEntry entry in contact.Phones)
                    strPhones += string.Format("{0}: {1}\r\n", entry.Label.ToUpper(), entry.Field);
            strPhones = strPhones.Trim();

            // Format our addresses section text
            string strAddresses = "";
            if(contact.StreetAddresses != null)
                foreach (iCloud.iCloudContactsContact.iCloudContactsContactAddress address in contact.StreetAddresses)
                {
                    strAddresses += string.Format(
                        @"Label: {0}
Street: {1}
City: {2}
Province/State: {3}
Country: {4}
Postal Code: {5}

",
            address.Label.ToUpper(),
            address.Field.Street,
            address.Field.City,
            address.Field.State,
            address.Field.Country,
            address.Field.PostalCode
                        );
                }
            strAddresses = strAddresses.Trim();

            // Format our related names section text
            string strRelated = "";
            if (contact.RelatedNames != null)
                foreach (iCloud.iCloudContactsStringEntry entry in contact.RelatedNames)
                    strRelated += string.Format("{0}: {1}\r\n", entry.Label.ToUpper(), entry.Field);
            strRelated = strRelated.Trim();

            txtContactInfo.Text = string.Format(@"ALIASES
========
First Name: {0}
Middle Name: {1}
Last Name: {2}
Nick Name: {3}
Phonetix First Name: {4}
Phonetic Last Name: {5}

PHONES
======
{6}

WORK
=====
Company: {7}
Department: {8}
Job Title: {9}

ADDRESSES
=========
{10}

DATES
=====
Birthday: {11}
{12}

RELATED
=======
{13}

NOTES
=====
{14}
",
 contact.FirstName,
 contact.MiddleName,
 contact.LastName,
 contact.NickName,
 contact.PhoneticFirstName,
 contact.PhoneticLastName,
 strPhones,
 contact.CompanyName,
 contact.Department,
 contact.JobTitle,
 strAddresses,
 contact.Birthday,
 strDates,
 strRelated,
 contact.Notes
 );

        }
    }
}
