//*****************************************************************************
// Copyright © 2014, David Pokora
//
// All rights reserved.
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Threading;
using System.Web;

namespace iCloudLib
{
    /// <summary>
    /// Exchanges information with the iCloud servers to obtain data for various provided web services.
    /// </summary>
    public class iCloud
    {
        #region Values

        // Private URLs
        private const string ICLOUD_HOME_URL = "https://www.icloud.com";
        private const string ICLOUD_LOGIN_URL = "https://setup.icloud.com/setup/ws/1/login";
        private string FindMyiPhoneInitClientUrl
        {
            get
            {
                if(loginResponse != null && loginResponse.WebServices != null && 
                    loginResponse.WebServices.FindMe != null && loginResponse.WebServices.FindMe.Url != null &&
                    loginResponse.DataSetInformation != null)
                    return string.Format("{0}/fmipservice/client/web/initClient?dsid={1}", loginResponse.WebServices.FindMe.Url, loginResponse.DataSetInformation.DataSetId);
                return null;
            }
        }
        private string FindMyiPhoneRefreshClientUrl
        {
            get
            {
                if(loginResponse != null && loginResponse.WebServices != null && 
                    loginResponse.WebServices.FindMe != null && loginResponse.WebServices.FindMe.Url != null &&
                    loginResponse.DataSetInformation != null)
                    return string.Format("{0}/fmipservice/client/web/refreshClient?dsid={1}", loginResponse.WebServices.FindMe.Url, loginResponse.DataSetInformation.DataSetId);
                return null;
            }
        }
        private string FindMyiPhonePlaySoundUrl
        {
            get
            {
                if (loginResponse != null && loginResponse.WebServices != null &&
                    loginResponse.WebServices.FindMe != null && loginResponse.WebServices.FindMe.Url != null &&
                    loginResponse.DataSetInformation != null)
                    return string.Format("{0}/fmipservice/client/web/playSound?dsid={1}", loginResponse.WebServices.FindMe.Url, loginResponse.DataSetInformation.DataSetId);
                return null;
            }
        }
        private string FindMyiPhoneRefererUrl
        {
            get
            {
                return ICLOUD_HOME_URL + "/applications/find/current/en-us/";
            }
        }
        private string ContactsMeCardUrl
        {
            get
            {
                if (loginResponse != null && loginResponse.WebServices != null &&
                    loginResponse.WebServices.Contacts != null && loginResponse.WebServices.Contacts.Url != null &&
                    loginResponse.DataSetInformation != null)
                    return string.Format("{0}/co/mecard/?dsid={1}", loginResponse.WebServices.Contacts.Url, loginResponse.DataSetInformation.DataSetId);
                return null;
            }
        }
        private string ContactsStartupUrl
        {
            get
            {
                if (loginResponse != null && loginResponse.WebServices != null &&
                    loginResponse.WebServices.Contacts != null && loginResponse.WebServices.Contacts.Url != null &&
                    loginResponse.DataSetInformation != null)
                {
                    string strSortOrder = ContactsSortOrder == iCloudContactSortOrder.FirstName ? "first,last" : "last,first";
                    return string.Format("{0}/co/startup?dsid={1}&locale={2}&order={3}", loginResponse.WebServices.Contacts.Url, loginResponse.DataSetInformation.DataSetId, contactsLocale, strSortOrder);
                }
                return null;
            }
        }
        private string ContactsChangeSetUrl
        {
            get
            {
                // In order to set, we need to first have started up.
                if (lastContactsResponse == null || lastContactsResponse.SyncToken == null || lastContactsResponse.PrefToken == null)
                    return null;

                // If we have a valid contacts url.
                if (loginResponse != null && loginResponse.WebServices != null &&
                    loginResponse.WebServices.Contacts != null && loginResponse.WebServices.Contacts.Url != null &&
                    loginResponse.DataSetInformation != null)
                    return string.Format("{0}/co/changeset?dsid={1}&prefToken={2}&syncToken={3}", loginResponse.WebServices.Contacts.Url, loginResponse.DataSetInformation.DataSetId, lastContactsResponse.PrefToken, Uri.EscapeDataString(lastContactsResponse.SyncToken));
                return null;
            }
        }
        private string ContactsSetAddUrl
        {
            get
            {
                // In order to set, we need to first have started up.
                if (lastContactsResponse == null || lastContactsResponse.SyncToken == null || lastContactsResponse.PrefToken == null)
                    return null;

                // If we have a valid contacts url.
                if (loginResponse != null && loginResponse.WebServices != null &&
                    loginResponse.WebServices.Contacts != null && loginResponse.WebServices.Contacts.Url != null &&
                    loginResponse.DataSetInformation != null)
                    return string.Format("{0}/co/contacts/card/?dsid={1}&prefToken={2}&syncToken={3}", loginResponse.WebServices.Contacts.Url, loginResponse.DataSetInformation.DataSetId, lastContactsResponse.PrefToken, Uri.EscapeDataString(lastContactsResponse.SyncToken));
                return null;
            }
        }
        private string ContactsSetEditUrl
        {
            get
            {
                // In order to set, we need to first have started up.
                if (lastContactsResponse == null || lastContactsResponse.SyncToken == null || lastContactsResponse.PrefToken == null)
                    return null;

                // If we have a valid contacts url.
                if (loginResponse != null && loginResponse.WebServices != null &&
                    loginResponse.WebServices.Contacts != null && loginResponse.WebServices.Contacts.Url != null &&
                    loginResponse.DataSetInformation != null)
                    return string.Format("{0}/co/contacts/card/?dsid={1}&method={2}&prefToken={3}&syncToken={4}", loginResponse.WebServices.Contacts.Url, loginResponse.DataSetInformation.DataSetId, "PUT", lastContactsResponse.PrefToken, Uri.EscapeDataString(lastContactsResponse.SyncToken));
                return null;
            }
        }
        private string ContactsSetDeleteUrl
        {
            get
            {
                // In order to set, we need to first have started up.
                if (lastContactsResponse == null || lastContactsResponse.SyncToken == null || lastContactsResponse.PrefToken == null)
                    return null;

                // If we have a valid contacts url.
                if (loginResponse != null && loginResponse.WebServices != null &&
                    loginResponse.WebServices.Contacts != null && loginResponse.WebServices.Contacts.Url != null &&
                    loginResponse.DataSetInformation != null)
                    return string.Format("{0}/co/contacts/card/?dsid={1}&method={2}&prefToken={3}&syncToken={4}", loginResponse.WebServices.Contacts.Url, loginResponse.DataSetInformation.DataSetId, "DELETE", lastContactsResponse.PrefToken, Uri.EscapeDataString(lastContactsResponse.SyncToken));
                return null;
            }
        }
        private string RefreshWebAuthUrl
        {
            get
            {
                if (loginResponse != null && loginResponse.WebServices != null &&
                  loginResponse.WebServices.Push != null && loginResponse.WebServices.Push.Url != null &&
                  loginResponse.DataSetInformation != null)
                    return string.Format("{0}/refreshWebAuth?dsid={1}", loginResponse.WebServices.Push.Url, loginResponse.DataSetInformation.DataSetId);
                return null;
            }
        }

        // Privately used variables for communication.
        private string contactsLocale = "en_US";
        private CookieContainer sessionCookies;
        private iCloudLoginResponse loginResponse;
        private iCloudFMIClientContext clientContext;
        private iCloudFMIServerContext serverContext;
        private iCloudContactsContactsResponse lastContactsResponse;
        private Thread findMyiPhoneThread;
        private Thread keepAliveThread;

        // Publically exposed variables.
        public bool IsConnected
        {
            get
            {
                return sessionCookies != null && loginResponse != null && loginResponse.DataSetInformation.StatusCode == 2;
            }
        }
        public iCloudDevice[] Devices { get; set; }
        public iCloudUserInfo UserInformation { get; set; }
        public iCloudUserPreferences UserPreferences { get; set; }
        public iCloudContactsMeCard ContactsMeCard { get; set; }
        public iCloudContactsContact[] Contacts
        {
            get
            {
                if (lastContactsResponse != null)
                    if (lastContactsResponse.Contacts != null)
                        return lastContactsResponse.Contacts;
                return null;
            }
        }
        public iCloudContactSortOrder ContactsSortOrder { get; set; }
        public int RefreshAuthenticationMilliseconds { get; set; }

        // Exceptions
        private static Exception notConnectedException = new Exception("Not connected to iCloud servers.");

        #endregion

        #region Constructor

        /// <summary>
        /// Our default constructor.
        /// </summary>
        public iCloud()
        {
            // Refresh web authentication every 2 minutes by default in this library. 
            RefreshAuthenticationMilliseconds = (60 * 1000) * 2;
        }
        /// <summary>
        /// Our default constructor.
        /// </summary>
        /// <param name="refreshAuthInterval">The interval at which we are refreshing our authentication with iCloud services.</param>
        public iCloud(int refreshAuthIntervalMilliseconds)
        {
            RefreshAuthenticationMilliseconds = refreshAuthIntervalMilliseconds;
        }

        #endregion

        #region Functions

        #region Connect / Disconnect / KeepAlive

        /// <summary>
        /// Given a username and passwords, attempts to connect to the iCloud servers.
        /// </summary>
        /// <param name="loginCredentials">The Apple account login credentials to authenticate to iCloud servers.</param>
        /// <returns>Returns a boolean indicating if the connection was successful.</returns>
        public bool Connect(iCloudLoginCredentials loginCredentials)
        {
            // Disconnect in case we were already connected, won't hurt.
            Disconnect();

            // Create our web client and set appropriate headers for our request.
            NetClient netClient = new NetClient(ICLOUD_HOME_URL, ICLOUD_HOME_URL);

            // Our login data is sent as a JSON string directly to the login URL.
            string loginData = Serialize(loginCredentials);
            try
            {
                // Set our content type header and post our login data.
                string result = netClient.POST(ICLOUD_LOGIN_URL, loginData);
                loginResponse = Deserialize<iCloudLoginResponse>(result);
                sessionCookies = netClient.CookieContainer;

                // If we could connect, attempt to quickly obtain some information to populate our class.
                if (IsConnected)
                {
                    // Attempt to obtain some device information, it can be refreshed by the callee later.
                    try
                    {
                        UpdateFindMyiPhone();
                    }
                    catch { }
                    // Attempt to obtain our contact information and personal contact card.
                    try
                    {
                        GetContacts();
                    }
                    catch { }

                    // Start our thread that will refresh our authentication cookie periodically.
                    StartKeepAliveThread();
                }

                return IsConnected;
            }
            catch(WebException webEx)
            {
                if(webEx.Status != WebExceptionStatus.ProtocolError)
                    throw webEx;
                return false;
            }
        }
        /// <summary>
        /// Disconnects from the iCloud servers.
        /// </summary>
        public void Disconnect()
        {
            // Determine if we were actually connected and are now disconnecting.
            bool wasConnected = IsConnected;

            // Stop our finding service if it's running.
            StopFindMyiPhone();
            StopKeepAliveThread();

            // Reset variables.
            Devices = null;
            UserInformation = null;
            UserPreferences = null;
            ContactsMeCard = null;
            sessionCookies = null;
            loginResponse = null;
            clientContext = null;
            serverContext = null;
            lastContactsResponse = null;

            // Call our disconnected event
            if (wasConnected && Disconnected != null)
                Disconnected();
        }

        /// <summary>
        /// This function starts a thread to keep the current connection open to iCloud servers by periodically refreshing our cookies.
        /// </summary>
        private void StartKeepAliveThread()
        {
            if (!IsConnected)
                throw notConnectedException;

            // Stop the keep alive thread if it's running
            StopKeepAliveThread();

            // Create our thread.
            keepAliveThread = new Thread(
                delegate()
                {
                    // Create our client.
                    NetClient netClient = new NetClient(sessionCookies, ICLOUD_HOME_URL, ICLOUD_HOME_URL);

                    // Continuously refresh our data
                    while (true)
                    {
                        // Wait first since this will be called when we first connect, no need to refresh at first.
                        Thread.Sleep(RefreshAuthenticationMilliseconds);

                        try
                        {
                            // Visit our refresh auth web page, which will give us a new cookie.
                            string strResult = netClient.GET(RefreshWebAuthUrl);
                        }
                        catch
                        {
                            // An error occurred, we're disconnected.
                            Disconnect();
                            break;
                        }
                    }
                }
            );

            // Execute the thread.
            keepAliveThread.Start();
        }
        /// <summary>
        /// This function stops the thread that keeps our authentication alive.
        /// </summary>
        private void StopKeepAliveThread()
        {
            // If we have a valid thread, abort it.
            if (keepAliveThread == null)
                return;
            keepAliveThread.Abort();

            // Wait until our thread stopped.
            while (keepAliveThread.ThreadState != ThreadState.Stopped && keepAliveThread.ThreadState != ThreadState.Aborted) { }
        }

        /// <summary>
        /// Our delegate to call upon when disconnected
        /// </summary>
        public delegate void iCloudDisconnectedHandler();
        /// <summary>
        /// The event that is fired upon when we were connected and disconnect.
        /// </summary>
        public event iCloudDisconnectedHandler Disconnected;

        #endregion

        #region Find My iPhone

        /// <summary>
        /// This function obtains our current client context for our Find My iPhone services or creates one if necessary.
        /// </summary>
        /// <returns>Returns a client context structure for Find My iPhone services.</returns>
        private iCloudFMIClientContext GetClientContext()
        {
            if(clientContext == null)
                clientContext =  new iCloudFMIClientContext("3.0", "iCloud Find (Web)", "2.0", 0, "Canada/Eastern", "0:16");
            return clientContext;
        }
        /// <summary>
        /// Calls for a single update from the Find My iPhone services, synchronously.
        /// </summary>
        private void UpdateFindMyiPhone()
        {
            if (!IsConnected)
                throw notConnectedException;

            // Obtain our client context
            iCloudFMIClientContext clientContext = GetClientContext();
            // If we're not connected, stop.
            if (!IsConnected)
                if (FindMyiPhoneFailed != null)
                    FindMyiPhoneFailed(null);

            // Determine if we're initializing or refreshing.
            bool initializing = serverContext == null;

            // Setup our data we're going to be sending, whether its for init or refresh.
            iCloudFMIClientRequest clientData = null;
            if (initializing)
                clientData = new iCloudFMIClientRequest(clientContext); // initialization
            else
                clientData = new iCloudFMIClientRequest(clientContext, serverContext); // refresh
            string clientRequestJson = Serialize(clientData);

            // Create our client
            NetClient netClient = new NetClient(sessionCookies, ICLOUD_HOME_URL, FindMyiPhoneRefererUrl);

            // Post our client context data to the servers.
            string postUrl = initializing ? FindMyiPhoneInitClientUrl : FindMyiPhoneRefreshClientUrl;
            string strResult = netClient.POST(postUrl, clientRequestJson);
            iCloudFMIClientResponse response = Deserialize<iCloudFMIClientResponse>(strResult);
            serverContext = response.ServerContext;

            // Set our server context's ID for when we send it back.
            if (serverContext != null)
                serverContext.Id = "server_ctx";

            // Set our information
            if (response.Devices != null)
                Devices = response.Devices;
            if (response.UserInformation != null)
                UserInformation = response.UserInformation;
            if (response.UserPreferences != null)
                UserPreferences = response.UserPreferences;

            // If our status is alright, we can call our update handler
            if (int.Parse(response.StatusCode) == 200)
            {
                if (FindMyiPhoneUpdate != null)
                    FindMyiPhoneUpdate(response);
            }
            else
            {
                if (FindMyiPhoneFailed != null)
                    FindMyiPhoneFailed(response);
            }

        }

        /// <summary>
        /// The delegate that is called upon when our asynchronous Find My iPhone operation is successfully refreshed.
        /// </summary>
        /// <param name="responseUpdate">The response of a client update request to find the device.</param>
        public delegate void FindMyiPhoneUpdateHandler(iCloudFMIClientResponse responseUpdate);
        /// <summary>
        /// The event in which our Find My iPhone service refreshes our device data asynchronously.
        /// </summary>
        public event FindMyiPhoneUpdateHandler FindMyiPhoneUpdate;
        /// <summary>
        /// The delegate that is called upon when our asynchronous Find My iPhone operation fails to refresh.
        /// </summary>
        /// <param name="responseUpdate">The response of a client update request to find the device. Either had an error code or is null because the connection failed.</param>
        public delegate void FindMyiPhoneFailedHandler(iCloudFMIClientResponse responseUpdate); 
        /// <summary>
        /// The event in which our Find My iPhone service fails to refresh our device data asynchronously.
        /// </summary>
        public event FindMyiPhoneFailedHandler FindMyiPhoneFailed;

        /// <summary>
        /// This function begins tracking an iPhone using Find My iPhone services through iCloud.
        /// The tracking updates are done asynchronously and will call the FindMyiPhone Handlers upon
        /// failure or update.
        /// </summary>
        /// <param name="refreshIntervalMS">The interval at which our thread will update information about our devices.</param>
        public void StartFindMyiPhone(int refreshIntervalMS=10000)
        {
            if (!IsConnected)
                throw notConnectedException;

            // First stop our thread in case it's already running
            StopFindMyiPhone();

            // Create our thread.
            findMyiPhoneThread = new Thread(
                delegate()
                {
                    // Continuously refresh our data
                    while (true)
                    {
                        // Update our find my iphone data.
                        UpdateFindMyiPhone();

                        // Wait our for the duration of our refresh interval.
                        Thread.Sleep(refreshIntervalMS);
                    }
                }
            );

            // Execute the thread.
            findMyiPhoneThread.Start();
        }
        /// <summary>
        /// This function aborts an existing asynchronous Find My iPhone operation.
        /// </summary>
        public void StopFindMyiPhone()
        {
            // If we have a valid thread, abort it.
            if (findMyiPhoneThread == null)
                return;
            findMyiPhoneThread.Abort();

            // Wait until our thread stopped.
            while (findMyiPhoneThread.ThreadState != ThreadState.Stopped && findMyiPhoneThread.ThreadState != ThreadState.Aborted) { }

            // Set our server context to null (so we reinitiate client communication instead of refreshing).
            serverContext = null;
        }


        /// <summary>
        /// Plays a sound to the given device to help locate it, while providing the given message.
        /// </summary>
        /// <param name="message">The message to display on-screen.</param>
        /// <param name="device">The device to play the sound on.</param>
        public void PlaySound(string message, iCloudDevice device)
        {
            PlaySound(message, device.DeviceId);
        }
        /// <summary>
        /// Plays a sound to the given device to help locate it, while providing the given message.
        /// </summary>
        /// <param name="message">The message to display on-screen.</param>
        /// <param name="deviceId">The device to play the sound on.</param>
        public void PlaySound(string message, string deviceId)
        {
            if (!IsConnected)
                throw notConnectedException;

            // If our server context is not valid, update.
            if (serverContext == null)
                UpdateFindMyiPhone();

            // Create our client data to post.
            iCloudFMIClientContext clientContext = GetClientContext();

            // If we're not connected, stop.
            if (!IsConnected)
                if (FindMyiPhoneFailed != null)
                    FindMyiPhoneFailed(null);

            // Setup our data we're going to be sending, whether its for init or refresh.
            iCloudFMIClientRequest clientData = new iCloudFMIClientRequest(clientContext, serverContext, deviceId, message);
            string clientRequestJson = Serialize(clientData);

            // Create our client
            NetClient netClient = new NetClient(sessionCookies, ICLOUD_HOME_URL, FindMyiPhoneRefererUrl);

            // Post our client context data to the servers.
            string strResult = netClient.POST(FindMyiPhonePlaySoundUrl, clientRequestJson);
            iCloudFMIClientResponse response = Deserialize<iCloudFMIClientResponse>(strResult);
            serverContext = response.ServerContext;

            // Set our server context's ID for when we send it back.
            if (serverContext != null)
            {
                serverContext.Id = "server_ctx";
            }
        }

        #endregion

        #region Contacts

        /// <summary>
        /// This function obtains our contact information for our iCloud account, such as the account's personal contact card if it has one,
        /// as well as the list of contacts the account has synchronized.
        /// </summary>
        /// <param name="sortOrder">The order in which the contacts will be sorted in.</param>
        public void GetContacts(iCloudContactSortOrder sortOrder=iCloudContactSortOrder.FirstName)
        {
            if (!IsConnected)
                throw notConnectedException;

            // Set our sort order
            ContactsSortOrder = sortOrder;

            // Create our client
            NetClient netClient = new NetClient(sessionCookies, ICLOUD_HOME_URL, ICLOUD_HOME_URL);

            // Post our client context data to the servers.
            string strResult = netClient.GET(ContactsMeCardUrl);
            ContactsMeCard = Deserialize<iCloudContactsMeCard>(strResult);

            // Now get our contacts list.
            strResult = netClient.GET(ContactsStartupUrl);
            lastContactsResponse = Deserialize<iCloudContactsContactsResponse>(strResult);
        }
        /// <summary>
        /// This function uploads a single contact to the Contacts services provided by iCloud. If the account identifier is not in the list
        /// of contacts, it will be added appropriately, otherwise updated.
        /// </summary>
        /// <param name="contact">The contact to upload to the Contacts webservices.</param>
        public void SetContact(iCloudContactsContact contact)
        {
            SetContacts(new iCloudContactsContact[] { contact });
        }
        /// <summary>
        /// This function uploads an array of contacts to the Contacts services provided by iCloud. If any accounts identifier is not in the list
        /// of contacts, it will be added appropriately, otherwise updated.
        /// </summary>
        /// <param name="contacts">The array of contacts to upload to the Contacts webservices.</param>
        public void SetContacts(iCloudContactsContact[] contacts)
        {
            iCloudContactsContact[] existingList = contacts.Where(w => Contacts.Select(s2 => s2.Id).Contains(w.Id)).ToArray();
            iCloudContactsContact[] newList = contacts.Where(w => !Contacts.Select(s2 => s2.Id).Contains(w.Id)).ToArray();
            SetContacts(existingList, false);
            SetContacts(newList, true);
        }
        /// <summary>
        /// This is the function that uploads an array of contacts to iCloud servers, indicating if they are all new accounts or all old.
        /// Because this function only either posts to the update or add url individually, it is unexposed to be called upon by public
        /// functions which determine what accounts are new and what are simply being updated.
        /// </summary>
        /// <param name="contacts">The array of contacts to upload to the Contacts webservices.</param>
        /// <param name="addingNew">A boolean indicating whether the array of contacts is entirely new contacts or entirely to be updated.</param>
        private void SetContacts(iCloudContactsContact[] contacts, bool addingNew)
        {
            if (!IsConnected)
                throw notConnectedException;
            if (contacts.Length == 0)
                return;

            // Create our client
            NetClient netClient = new NetClient(sessionCookies, ICLOUD_HOME_URL, ICLOUD_HOME_URL);
            //string strResult = netClient.POST(ContactsChangeSetUrl, "", false);

            // Create our iCloudContact message
            iCloudContactsUpdateRequest contactsData = new iCloudContactsUpdateRequest(contacts);
            string contactsDataJson = Serialize(contactsData);

            // Post our requested contact data to the servers.
            string postUrl = addingNew ? ContactsSetAddUrl : ContactsSetEditUrl;
            string strResult = netClient.POST(postUrl, contactsDataJson, false);
            iCloudContactsContactsResponse contactsResponse = Deserialize<iCloudContactsContactsResponse>(strResult);

            // Simply update our response so our sync token and other information is updated
            if (lastContactsResponse != null)
            {
                lastContactsResponse.ContactsOrderList = contactsResponse.ContactsOrderList;
                lastContactsResponse.Collections = contactsResponse.Collections;
                lastContactsResponse.HeaderPositions = contactsResponse.HeaderPositions;
                lastContactsResponse.MeCardId = contactsResponse.MeCardId;
                lastContactsResponse.PrefToken = contactsResponse.PrefToken;
                lastContactsResponse.SyncToken = contactsResponse.SyncToken;

                // Obtain a dictionary of our contacts to update to keep track of them faster
                Dictionary<string, iCloudContactsContact> contactsUpdated = new Dictionary<string,iCloudContactsContact>();
                foreach(iCloudContactsContact updatedContact in contactsResponse.Contacts)
                    contactsUpdated[updatedContact.Id] = updatedContact;

                // Create a new list with our updated and added contacts in our new sort order.
                iCloudContactsContact[] newList = new iCloudContactsContact[lastContactsResponse.ContactsOrderList.Length];
                for (int i = 0; i < newList.Length; i++)
                {
                    // If we updated a contact, add the updated one to the list, otherwise our original one.
                    string id = lastContactsResponse.ContactsOrderList[i];
                    if (contactsUpdated.ContainsKey(id))
                        newList[i] = contactsUpdated[id];
                    else
                        newList[i] = Contacts.Where(x => x.Id == id).First();
                }
                
                // Set our list as the new list.
                lastContactsResponse.Contacts = newList;
            }
        }
        /// <summary>
        /// This function requests a single contact be deleted by iCloud Contacts services. Contacts to delete are
        /// indexed by Id and ETag, therefore these values must be valid to remove a contact.
        /// </summary>
        /// <param name="contact">The contact to request deletion for with iCloud Contacts services.</param>
        public void DeleteContact(iCloudContactsContact contact)
        {
            DeleteContacts(new iCloudContactsContact[] { contact });
        }
        /// <summary>
        /// This function requests an array of contacts be deleted by iCloud Contacts services. Contacts to delete are
        /// indexed by Id and ETag, therefore these values must be valid to remove a contact.
        /// </summary>
        /// <param name="contacts">The array of contacts to request deletion for with iCloud Contacts services.</param>
        public void DeleteContacts(iCloudContactsContact[] contacts)
        {
            if (!IsConnected)
                throw notConnectedException;
            if (contacts.Length == 0)
                return;

            // Create our client
            NetClient netClient = new NetClient(sessionCookies, ICLOUD_HOME_URL, ICLOUD_HOME_URL);

            // Create our iCloudContact message
            iCloudContactsDeleteRequest contactsData = new iCloudContactsDeleteRequest(contacts);
            string contactsDataJson = Serialize(contactsData);

            // Post our contact delete data to the servers.
            string strResult = netClient.POST(ContactsSetDeleteUrl, contactsDataJson, false);
            iCloudContactsContactsResponse contactsResponse = Deserialize<iCloudContactsContactsResponse>(strResult);

            // Simply update our response so our sync token and other information is updated
            if (lastContactsResponse != null)
            {
                lastContactsResponse.ContactsOrderList = contactsResponse.ContactsOrderList;
                lastContactsResponse.Collections = contactsResponse.Collections;
                lastContactsResponse.HeaderPositions = contactsResponse.HeaderPositions;
                lastContactsResponse.MeCardId = contactsResponse.MeCardId;
                lastContactsResponse.PrefToken = contactsResponse.PrefToken;
                lastContactsResponse.SyncToken = contactsResponse.SyncToken;

                // Check if we need to update our contact list
                if (contactsResponse.DeletedContacts != null && contactsResponse.DeletedContacts.ContactIds != null)
                    // Create our new list of contacts
                    lastContactsResponse.Contacts = Contacts.Where(x => !contactsResponse.DeletedContacts.ContactIds.Contains(x.Id)).ToArray();
            }
        }

        #endregion

        #region Serialization

        /// <summary>
        /// Serialized our given DataContract object into a JSON string.
        /// </summary>
        /// <param name="obj">The object to serialize.</param>
        /// <returns>Returns a JSON string representing the DataContract object.</returns>
        private static string Serialize(object obj)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(obj.GetType());
            MemoryStream ms = new MemoryStream();
            serializer.WriteObject(ms, obj);
            StreamReader sr = new StreamReader(ms);
            sr.BaseStream.Position = 0;
            string jsonStr = sr.ReadToEnd();
            sr.Close();
            ms.Close();
            return jsonStr;
        }
        /// <summary>
        /// Deserializes our given JSON string into a DataContract Object.
        /// </summary>
        /// <typeparam name="T">The type our object pertains to.</typeparam>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>Returns the object the JSON string represents.</returns>
        private static T Deserialize<T>(string json)
        {
            var instance = Activator.CreateInstance<T>();
            using (var ms = new MemoryStream(Encoding.Unicode.GetBytes(json)))
            {
                var serializer = new DataContractJsonSerializer(instance.GetType());
                return (T)serializer.ReadObject(ms);
            }
        }

        #endregion

        #endregion

        #region Enums
        /// <summary>
        /// Describes the order in which a contact list should be sorted.
        /// </summary>
        public enum iCloudContactSortOrder
        {
            FirstName,
            LastName
        }
        #endregion

        #region Classes

        #region Authentication

        /// <summary>
        /// The credentials used to request authentication to iCloud servers.
        /// </summary>
        [DataContract]
        public class iCloudLoginCredentials
        {
            /// <summary>
            /// This property describes our Apple ID to authenticate against iCloud servers into.
            /// </summary>
            [DataMember(Name = "apple_id")]
            public string AppleId { get; set; }
            /// <summary>
            /// This property describes the password for the provided Apple ID within this structure.
            /// </summary>
            [DataMember(Name = "password")]
            public string Password { get; set; }
            /// <summary>
            /// This property indicates whether we are interested in an extended login or not.
            /// </summary>
            [DataMember(Name = "extended_login")]
            public bool ExtendedLogin { get; set; }

            /// <summary>
            /// Our default constructor for our login credentials structure.
            /// </summary>
            /// <param name="appleId">The Apple ID to authenticate to.</param>
            /// <param name="pass">The passphrase for the given Apple ID.</param>
            /// <param name="extendedLogin">A boolean indicating if we are interested in an extended login period.</param>
            public iCloudLoginCredentials(string appleId, string pass, bool extendedLogin=false)
            {
                AppleId = appleId;
                Password = pass;
                ExtendedLogin = extendedLogin;
            }
        }

        /// <summary>
        /// The response obtained from iCloud servers upon authentication, providing information for services and the user's account.
        /// </summary>
        [DataContract]
        public class iCloudLoginResponse
        {
            /// <summary>
            /// Indicates whether our login was an extended login session.
            /// </summary>
            [DataMember(Name = "isExtendedLogin")]
            public bool IsExtendedLogin { get; set; }
            /// <summary>
            /// This structure describes our web services provided by iCloud such as calendars, contacts, findme, etc.
            /// </summary>
            [DataMember(Name = "webservices")]
            public iCloudWebServices WebServices { get; set; }
            /// <summary>
            /// Contains a set of data describing our user we authenticated as.
            /// </summary>
            [DataMember(Name = "dsInfo")]
            public iCloudDataSetInformation DataSetInformation { get; set; }
            /// <summary>
            /// The order of our applications/webservices provided by iCloud.
            /// </summary>
            [DataMember(Name = "appsOrder")]
            public string[] AppsOrder { get; set; }
            /// <summary>
            /// Provides a structure for which applications we are qualified to take place in 
            /// beta tests for.
            /// </summary>
            [DataMember(Name = "apps")]
            public iCloudApplications Applications { get; set; }
            /// <summary>
            /// This structure describes our location information for our authentication.
            /// </summary>
            [DataMember(Name = "requestInfo")]
            public iCloudRequestInformation RequestInformation { get; set; }
            /// <summary>
            /// This describes the version of our authentication/handshaking protocol presumably.
            /// </summary>
            [DataMember(Name = "version")]
            public long Version { get; set; }

            /// <summary>
            /// This structure indicates the status of provided web services and how they can be accessed.
            /// </summary>
            [DataContract]
            public class iCloudWebServices
            {
                [DataMember(Name = "reminders")]
                public iCloudWebService Reminders { get; set; }
                [DataMember(Name = "ubiquity")]
                public iCloudWebService Ubiquity { get; set; }
                [DataMember(Name = "account")]
                public iCloudAccountWebService Account { get; set; }
                [DataMember(Name = "streams")]
                public iCloudWebService Streams { get; set; }
                [DataMember(Name = "keyvalue")]
                public iCloudWebService KeyValue { get; set; }
                [DataMember(Name = "push")]
                public iCloudWebService Push { get; set; }
                [DataMember(Name = "findme")]
                public iCloudWebService FindMe { get; set; }
                [DataMember(Name = "contacts")]
                public iCloudWebService Contacts { get; set; }
                [DataMember(Name = "calendar")]
                public iCloudWebService Calendar { get; set; }

                /// <summary>
                /// This class contains information for iCloud web services such as contacts, find me, etc.
                /// Should be extended for services with additional properties.
                /// </summary>
                [DataContract]
                public class iCloudWebService
                {
                    /// <summary>
                    /// The status of our web service. "active" should describe a healthy web service.
                    /// </summary>
                    [DataMember(Name = "status")]
                    public string Status;
                    /// <summary>
                    /// The Url provided for the host which serves our web service.
                    /// </summary>
                    [DataMember(Name = "url")]
                    public string Url;
                }

                [DataContract]
                public class iCloudAccountWebService : iCloudWebService
                {
                    [DataMember(Name = "iCloudEnv")]
                    public iCloudCloudEnvironment iCloudEnvironment { get; set; }

                    [DataContract]
                    public class iCloudCloudEnvironment
                    {
                        [DataMember(Name = "shortId")]
                        public string ShortId { get; set; }
                    }
                }
            }
            /// <summary>
            /// This structure describes the user we authenticate as.
            /// </summary>
            [DataContract]
            public class iCloudDataSetInformation
            {
                /// <summary>
                /// A boolean which indicates whether our primary email address was verified or not.
                /// </summary>
                [DataMember(Name = "primaryEmailVerified")]
                public bool PrimaryEmailVerified { get; set; }
                /// <summary>
                /// A property which describes our accounts last name on file.
                /// </summary>
                [DataMember(Name = "lastName")]
		        public string LastName { get; set; }
                /// <summary>
                /// A property which describes our alias for iCloud. Will be blank if not set.
                /// </summary>
                [DataMember(Name = "iCloudAppleIdAlias")]
		        public string iCloudAppleIdAlias { get; set; }
                /// <summary>
                /// A property which describes our alias. Will be blank if not set.
                /// </summary>
                [DataMember(Name = "appleIdAlias")]
		        public string AppleIdAlias { get; set; }
                /// <summary>
                /// A property which describes if we have a device which can utilize iCloud connected.
                /// </summary>
                [DataMember(Name = "hasICloudQualifyingDevice")]
		        public bool HasiCloudQualifyingDevice { get; set; }
                /// <summary>
                /// A property describing the Apple ID address we authenticated to.
                /// </summary>
                [DataMember(Name = "appleId")]
		        public string AppleId { get; set; }
                /// <summary>
                /// Indicates if the provided Apple ID is enrolled in a developer program.
                /// </summary>
                [DataMember(Name = "isPaidDeveloper")]
		        public bool IsPaidDeveloper { get; set; }
                [DataMember(Name = "gilligan-invited")]
		        public bool InvitedToGilligan { get; set; }
                /// <summary>
                /// Our data set identifier, which uniquely identifies our account, used throughout server communication.
                /// </summary>
                [DataMember(Name = "dsid")]
		        public ulong DataSetId { get; set; }
                /// <summary>
                /// Our primary email linked to the iCloud account, typically the same as the AppleId property.
                /// </summary>
                [DataMember(Name = "primaryEmail")]
		        public string PrimaryEmail { get; set; }
                /// <summary>
                /// A status code describing the status of our iCloud account. 2 should indicate a properly setup account with iCloud that was authenticated to.
                /// </summary>
                [DataMember(Name = "statusCode")]
		        public long StatusCode { get; set; }
                /// <summary>
                /// Describes our iCloud accounts full name. This is simply the first name concatenated with the second name with a space between.
                /// </summary>
                [DataMember(Name = "fullName")]
		        public string FullName { get; set; }
                /// <summary>
                /// Indicates whether the account is locked or not.
                /// </summary>
                [DataMember(Name = "locked")]
		        public bool Locked { get; set; }
                /// <summary>
                /// A property which describes our accounts first name on file.
                /// </summary>
                [DataMember(Name = "firstName")]
		        public string FirstName { get; set; }
                /// <summary>
                /// A property which describes a collection of aliases for the account.
                /// </summary>
                [DataMember(Name = "appleIdAliases")]
		        public string[] AppleIdAliases { get; set; }
            }
            /// <summary>
            /// This structure describes which applications we are privileged to beta test for.
            /// </summary>
            [DataContract]
            public class iCloudApplications
            {
                [DataMember(Name = "mail")]
                public Application Mail { get; set; }
                [DataMember(Name = "reminders")]
                public Application Reminders { get; set; }
                [DataMember(Name = "numbers")]
                public Application Numbers { get; set; }
                [DataMember(Name = "documents")]
                public Application Documents { get; set; }
                [DataMember(Name = "pages")]
                public Application Pages { get; set; }
                [DataMember(Name = "notes")]
                public Application Notes { get; set; }
                [DataMember(Name = "find")]
                public Application Find { get; set; }
                [DataMember(Name = "contacts")]
                public Application Contacts { get; set; }
                [DataMember(Name = "calendar")]
                public Application Calendar { get; set; }
                [DataMember(Name = "keynote")]
                public Application Keynote { get; set; }

                [DataContract]
                public class Application
                {
                    /// <summary>
                    /// This property indicates if we are qualified to partake in beta testing for the application.
                    /// </summary>
                    [DataMember(Name = "isQualifiedForBeta")]
                    public bool IsQualifiedForBeta { get; set; }
                }
            }
            /// <summary>
            /// This structure describes our location information for our authentication.
            /// </summary>
            [DataContract]
            public class iCloudRequestInformation
            {
                /// <summary>
                /// The region we authenticate from, usually a state or province, described in a two letter form, ex: "NY" = New York.
                /// </summary>
                [DataMember(Name = "region")]
                public string Region { get; set; }
                /// <summary>
                /// The timezone we are authenticating within.
                /// </summary>
                [DataMember(Name = "timeZone")]
                public string TimeZone { get; set; }
                /// <summary>
                /// The country we are authenticating from, described in a two character form, ex: "CA" = Canada.
                /// </summary>
                [DataMember(Name = "country")]
                public string Country { get; set; }
            }
        }

        #endregion

        #region Find My iPhone

        /// <summary>
        /// This structure describes a request to Find My iPhone services for various functions such as tracking and playing sounds.
        /// </summary>
        [DataContract]
        public class iCloudFMIClientRequest
        {
            /// <summary>
            /// A structure of information describing our client and it's protocol it is using to establish communication.
            /// </summary>
            [DataMember(Name = "clientContext")]
            public iCloudFMIClientContext ClientContext { get; set; }
            /// <summary>
            /// Describes various information regarding the servers requests, such as timestamps, statuses, tokens, and session information.
            /// </summary>
            [DataMember(Name = "serverContext")]
            public iCloudFMIServerContext ServerContext { get; set; }
            /// <summary>
            /// A property which is only set when requesting to play a sound on the device through Find My iPhone, describes which device to play the sound on.
            /// </summary>
            [DataMember(Name = "device")]
            public string DeviceIdBase64 { get; set; }
            /// <summary>
            /// A property which is only set when requesting to play a sound on the device through Find My iPhone, describes the message to display when the sound is played on the device.
            /// </summary>
            [DataMember(Name = "subject")]
            public string Subject { get; set; }

            public iCloudFMIClientRequest(iCloudFMIClientContext clientContext)
            {
                ClientContext = clientContext;
            }
            public iCloudFMIClientRequest(iCloudFMIClientContext clientContext, iCloudFMIServerContext serverContext)
            {
                ClientContext = clientContext;
                ServerContext = serverContext;
            }
            public iCloudFMIClientRequest(iCloudFMIClientContext clientContext, iCloudFMIServerContext serverContext, string deviceIdBase64, string subject)
            {
                ClientContext = clientContext;
                ServerContext = serverContext;
                DeviceIdBase64 = deviceIdBase64;
                Subject = subject;
            }
        }

        /// <summary>
        /// This structure describes a response from Find My iPhone services for various functions such as tracking and playing sounds.
        /// </summary>
        [DataContract]
        public class iCloudFMIClientResponse
        {
            /// <summary>
            /// An augmented version of typical HTTP Status Codes indicating the status of our request.
            /// </summary>
            [DataMember(Name = "statusCode")]
            public string StatusCode { get; set; }
            /// <summary>
            /// This property describes a list of devices associated with the iCloud account.
            /// </summary>
            [DataMember(Name = "content")]
            public iCloudDevice[] Devices { get; set; }
            /// <summary>
            /// A structure describing our iCloud account's first and last name.
            /// </summary>
            [DataMember(Name = "userInfo")]
            public iCloudUserInfo UserInformation { get; set; }
            /// <summary>
            /// A structure describing our users preferences for iCloud such as the current selected device.
            /// </summary>
            [DataMember(Name = "userPreferences")]
            public iCloudUserPreferences UserPreferences { get; set; }
            /// <summary>
            /// Describes various information regarding the servers requests, such as timestamps, statuses, tokens, and session information.
            /// </summary>
            [DataMember(Name = "serverContext")]
            public iCloudFMIServerContext ServerContext { get; set; }
        }

        /// <summary>
        /// A structure of information describing our client and it's protocol it is using to establish communication.
        /// </summary>
        [DataContract]
        public class iCloudFMIClientContext
        {
            /// <summary>
            /// A property describing our protocols api version number.
            /// </summary>
            [DataMember(Name = "apiVersion")]
            public string ApiVersion { get; set; }
            /// <summary>
            /// A property describing our application name we are accessing.
            /// </summary>
            [DataMember(Name = "appName")]
            public string ApplicationName { get; set; }
            /// <summary>
            /// A property indicating the version of the application we are requesting to access.
            /// </summary>
            [DataMember(Name = "appVersion")]
            public string ApplicationVersion { get; set; }
            /// <summary>
            /// Indicates the amount of time spent inactive.
            /// </summary>
            [DataMember(Name = "inactiveTime")]
            public ulong InactiveTime { get; set; }
            /// <summary>
            /// A property indicating the client's time zone.
            /// </summary>
            [DataMember(Name = "timezone")]
            public string TimeZone { get; set; }
            /// <summary>
            /// A property describing our web statistics, which resembles a time stamp string.
            /// </summary>
            [DataMember(Name = "webStats")]
            public string WebStatistics { get; set; }

            /// <summary>
            /// Our default constructor for our client context.
            /// </summary>
            /// <param name="apiVersion">Our protocol api version.</param>
            /// <param name="appName">The application name we are accessing.</param>
            /// <param name="appVersion">The version of the application we are accessing (or rather the protocol version we are using to attempt to do so).</param>
            /// <param name="inactiveTime">The time spent inactive on the page.</param>
            /// <param name="timezone">The timezone pertaining to our client.</param>
            /// <param name="webStats"></param>
            public iCloudFMIClientContext(string apiVersion, string appName, string appVersion, ulong inactiveTime, string timezone, string webStats)
            {
                ApiVersion = apiVersion;
                ApplicationName = appName;
                ApplicationVersion = appVersion;
                InactiveTime = inactiveTime;
                TimeZone = timezone;
                WebStatistics = webStats;
            }
        }

        /// <summary>
        /// Describes various information regarding the servers requests, such as timestamps, statuses, tokens, and session information.
        /// </summary>
        [DataContract]
        public class iCloudFMIServerContext
        {
            [DataMember(Name = "minTrackLocThresholdInMts")]
            public ulong MinTrackLocThresholdInMins { get; set; }
            [DataMember(Name = "prefsUpdateTime")]
            public ulong PreferencesUpdateTime { get; set; }
            [DataMember(Name = "maxDeviceLoadTime")]
            public ulong MaximumDeviceLoadTime { get; set; }
            [DataMember(Name = "authToken")]
            public object AuthorizationToken { get; set; }
            [DataMember(Name = "classicUser")]
            public bool ClassicUser { get; set; }
            [DataMember(Name = "sessionLifespan")]
            public ulong SessionLifeSpan { get; set; }
            [DataMember(Name = "serverTimestamp")]
            public ulong ServerTimeStamp { get; set; }
            [DataMember(Name = "enableMapStats")]
            public bool MapStatisticsEnabled { get; set; }
            [DataMember(Name = "imageBaseUrl")]
            public string ImageBaseUrl { get; set; }
            [DataMember(Name = "deviceLoadStatus")]
            public string DeviceLoadStatus { get; set; }
            [DataMember(Name = "preferredLanguage")]
            public string PreferredLanguage { get; set; }
            [DataMember(Name = "clientId")]
            public string ClientIdBase64 { get; set; }
            [DataMember(Name = "lastSessionExtensionTime")]
            public object LastSessionExtensionTime { get; set; }
            [DataMember(Name = "trackInfoCacheDurationInSecs")]
            public long TrackingInfoCacheDurationInSeconds { get; set; }
            [DataMember(Name = "isHSA")]
            public bool IsHSA { get; set; }
            [DataMember(Name = "timezone")]
            public iCloudTimeZone TimeZone { get; set; }
            [DataMember(Name = "callbackIntervalInMS")]
            public long CallbackIntervalInMilliseconds { get; set; }
            [DataMember(Name = "cloudUser")]
            public bool IsCloudUser { get; set; }
            [DataMember(Name = "validRegion")]
            public bool IsValidRegion { get; set; }
            [DataMember(Name = "maxLocatingTime")]
            public long MaxLocatingTime { get; set; }
            [DataMember(Name = "prsId")]
            public ulong PersonId { get; set; } // presumably this is what prs stands for.
            [DataMember(Name = "macCount")]
            public ulong MacCount { get; set; }
            [DataMember(Name = "server_ctx")]
            public string Id { get; set; }

            [DataContract]
            public class iCloudTimeZone
            {
                [DataMember(Name = "tzCurrentName")]
                public string CurrentName { get; set; }
                [DataMember(Name = "previousTransition")]
                public ulong PreviousTransition { get; set; }
                [DataMember(Name = "previousOffset")]
                public long PreviousOffset { get; set; }
                [DataMember(Name = "currentOffset")]
                public long CurrentOffset { get; set; }
                [DataMember(Name = "tzName")]
                public string Name { get; set; }
            }
        }

        /// <summary>
        /// This structure describes a device paired with iCloud which the user can check the status of, or possibly track.
        /// </summary>
        [DataContract]
        public class iCloudDevice
        {
            /// <summary>
            /// Indicates if our device can be wiped after it is locked.
            /// </summary>
            [DataMember(Name = "canWipeAfterLock")]
            public bool CanWipeAfterLock { get; set; }
            [DataMember(Name = "remoteWipe")]
            public object RemoteWipe { get; set; }
            [DataMember(Name = "locFoundEnabled")]
            public bool LocationFoundEnabled { get; set; } // ???
            /// <summary>
            /// This structure describes our devices location.
            /// </summary>
            [DataMember(Name = "location")]
            public iCloudDeviceLocation Location { get; set; }
            [DataMember(Name = "deviceModel")]
            public string DeviceModel { get; set; }
            [DataMember(Name = "remoteLock")]
            public object RemoteLock { get; set; }
            /// <summary>
            /// Indicates whether or not the device is tied to activation locks.
            /// </summary>
            [DataMember(Name = "activationLocked")]
            public bool IsActivationLocked { get; set; }
            /// <summary>
            /// Indicates whether or not location services are enabled for the device.
            /// </summary>
            [DataMember(Name = "locationEnabled")]
            public bool LocationEnabled { get; set; }
            /// <summary>
            /// Indicates the devices raw model, such as "iPhone6,1".
            /// </summary>
            [DataMember(Name = "rawDeviceModel")]
            public string RawDeviceModel { get; set; }
            /// <summary>
            /// Indicates the device model's display name, such as "iPhone".
            /// </summary>
            [DataMember(Name = "modelDisplayName")]
            public string ModelDisplayName { get; set; }
            /// <summary>
            /// Indicates whether or not the device is capable of entering lost mode.
            /// </summary>
            [DataMember(Name = "lostModeCapable")]
            public bool IsLostModeCapable { get; set; }
            /// <summary>
            /// Indicates the device Id, base64 encoded.
            /// </summary>
            [DataMember(Name = "id")]
            public string DeviceId { get; set; }
            /// <summary>
            /// Indicates the device's actual display name, not simply its class, such as "iPhone 5s".
            /// </summary>
            [DataMember(Name = "deviceDisplayName")]
            public string DeviceDisplayName { get; set; }
            [DataMember(Name = "darkWake")]
            public bool DarkWake { get; set; } // ???
            /// <summary>
            /// Indicates whether or not our device is capable of location tracking.
            /// </summary>
            [DataMember(Name = "locationCapable")]
            public bool IsLocationCapable { get; set; }
            /// <summary>
            /// Indicates the maximum length of characters the device can receive in a single message.
            /// </summary>
            [DataMember(Name = "maxMsgChar")]
            public ulong MaximumMessageCharacterLength { get; set; }
            /// <summary>
            /// Indicates the devices name given to it by the owner.
            /// </summary>
            [DataMember(Name = "name")]
            public string DeviceName { get; set; }
            /// <summary>
            /// Represents the battery level percentage in a decimal format from 0-1.
            /// </summary>
            [DataMember(Name = "batteryLevel")]
            public double BatteryLevel { get; set; } // expressed in 0-1.
            [DataMember(Name = "features")]
            public object Features { get; set; } // needs replacement with the actual struct.
            /// <summary>
            /// Indicates the class of our device, such as MacBookPro or iPhone.
            /// </summary>
            [DataMember(Name = "deviceClass")]
            public string DeviceClass { get; set; }
            /// <summary>
            /// Indicates if a wipe on the device is currently in progress.
            /// </summary>
            [DataMember(Name = "wipeInProgress")]
            public bool IsWipeInProgress { get; set; }
            /// <summary>
            /// Indicates the length of our pass code on the device.
            /// </summary>
            [DataMember(Name = "passcodeLength")]
            public uint PasscodeLength { get; set; }
            [DataMember(Name = "mesg")]
            public object Mesg { get; set; } // ???
            /// <summary>
            /// Indicates whether or not the device is a Mac computer.
            /// </summary>
            [DataMember(Name = "isMac")]
            public bool IsMac { get; set; }
            [DataMember(Name = "snd")]
            public object Sound { get; set; } // needs replacement with the actual struct (iCloud play sound status)
            /// <summary>
            /// Indicates whether our device is being located or not.
            /// </summary>
            [DataMember(Name = "isLocating")]
            public bool IsLocating { get; set; }
            [DataMember(Name = "trackingInfo")]
            public object TrackingInformation { get; set; } // ???
            /// <summary>
            /// Indicates a code denoting the device's color.
            /// </summary>
            [DataMember(Name = "deviceColor")]
            public string DeviceColor { get; set; }
            /// <summary>
            /// Indicates the status of the battery such as Unknown, Charging, NotCharging.
            /// </summary>
            [DataMember(Name = "batteryStatus")]
            public string BatteryStatus { get; set; }
            /// <summary>
            /// Indicates a HTTP status like code indicating the device's status.
            /// </summary>
            [DataMember(Name = "deviceStatus")]
            public string DeviceStatus { get; set; }
            /// <summary>
            /// A timestamp representing when the device was wiped.
            /// </summary>
            [DataMember(Name = "wipedTimestamp")]
            public object WipedTimestamp { get; set; } // ???
            /// <summary>
            /// A timestamp indicating when the device was locked.
            /// </summary>
            [DataMember(Name = "lockedTimestamp")]
            public object LockedTimestamp { get; set; } // ???
            [DataMember(Name = "msg")]
            public object Message { get; set; } // needs replacement with the actual struct (iCloud message status)
            /// <summary>
            /// Indicates the time at which the device was set as lost.
            /// </summary>
            [DataMember(Name = "lostTimestamp")]
            public string LostTimestamp { get; set; }
            /// <summary>
            /// Indicates whether or not the device is in lost mode.
            /// </summary>
            [DataMember(Name = "lostModeEnabled")]
            public bool IsLostModeEnabled { get; set; }
            [DataMember(Name = "thisDevice")]
            public bool ThisDevice { get; set; } // ???
            [DataMember(Name = "lostDevice")]
            public object LostDevice { get; set; } // ???

            /// <summary>
            /// This structure describes our devices location.
            /// </summary>
            [DataContract]
            public class iCloudDeviceLocation
            {
                /// <summary>
                /// A timestamp describing when the device was found.
                /// </summary>
                [DataMember(Name = "timeStamp")]
                public ulong TimeStamp { get; set; }
                [DataMember(Name = "locationType")]
                public object LocationType { get; set; } // ???
                /// <summary>
                /// Describes the method in which the device is being tracked ("Wifi", etc).
                /// </summary>
                [DataMember(Name = "positionType")]
                public object PositionType { get; set; }
                /// <summary>
                /// The horizontal accuracy (0-100) of the tracking capability.
                /// </summary>
                [DataMember(Name = "horizontalAccuracy")]
                public double HorizontalAccuracy { get; set; }
                /// <summary>
                /// Indicates whether the location tracking was successfully completed.
                /// </summary>
                [DataMember(Name = "locationFinished")]
                public bool LocationFinished { get; set; }
                /// <summary>
                /// Indicates whether our coordinates are inaccurate.
                /// </summary>
                [DataMember(Name = "isInaccurate")]
                public bool IsInaccurate { get; set; }
                /// <summary>
                /// Indicates the longitude coordinate for the device.
                /// </summary>
                [DataMember(Name = "longitude")]
                public double Longitude { get; set; }
                /// <summary>
                /// Indicates the latitude coordinate for the device.
                /// </summary>
                [DataMember(Name = "latitude")]
                public double Latitude { get; set; }
                /// <summary>
                /// Indicates if the location provided is not recent.
                /// </summary>
                [DataMember(Name = "isOld")]
                public bool IsOld { get; set; }
            }
        }

        /// <summary>
        /// A structure describing our iCloud account's first and last name.
        /// </summary>
        [DataContract]
        public class iCloudUserInfo
        {
            /// <summary>
            /// A property describing our accounts first name.
            /// </summary>
            [DataMember(Name = "firstName")]
            public string FirstName { get; set; }
            /// <summary>
            /// A property describing our accounts last name.
            /// </summary>
            [DataMember(Name = "lastName")]
            public string LastName { get; set; }
        }

        /// <summary>
        /// This structure describes the iCloud user's preferences such as a selected device.
        /// </summary>
        [DataContract]
        public class iCloudUserPreferences
        {
            [DataMember(Name = "activationProhibitedDevices")]
            public object ActivationProhibitedDevices { get; set; } // likely has to do with lockout with lost phones.
            /// <summary>
            /// Indicates whether an email for a devices activation was sent or not.
            /// </summary>
            [DataMember(Name = "activationUpgradeEmailSent")]
            public bool ActivationUpgradeEmailSent { get; set; }
            [DataMember(Name = "webPrefs")]
            public iCloudWebPrefs WebPreferences { get; set; }
            [DataMember(Name = "lastUpdatedTime")]
            public ulong LastUpdatedTime { get; set; }
            [DataMember(Name = "builder")]
            public object Builder { get; set; } // ???

            [DataContract]
            public class iCloudWebPrefs
            {
                [DataMember(Name = "id")]
                public string Id { get; set; }
                /// <summary>
                /// The device currently selected in our web services.
                /// </summary>
                [DataMember(Name = "selectedDeviceId")]
                public string SelectedDeviceId { get; set; }
            }
        }

        #endregion

        #region Contacts

        /// <summary>
        /// This structure describes the current account's personal contact structure.
        /// </summary>
        [DataContract]
        public class iCloudContactsMeCard
        {
            /// <summary>
            /// A property describing an identifier pertaining to the Contact card for the account.
            /// </summary>
            [DataMember(Name = "meCardId")]
            public string Id { get; set; }
            /// <summary>
            /// The list of contacts returned pertaining to the account. This should just be the single account card the user indicated is themselves if done correctly.
            /// </summary>
            [DataMember(Name = "contacts")]
            public iCloudContactsContact[] Contacts { get; set; }
        }

        /// <summary>
        /// This structure describes a response to requesting initialization with the contacts web service, which contains our accounts full contact list and sorting options.
        /// </summary>
        [DataContract]
        public class iCloudContactsContactsResponse
        {
            /// <summary>
            /// The order of our contacts as a list of contact identifiers.
            /// </summary>
            [DataMember(Name = "contactsOrder")]
            public string[] ContactsOrderList { get; set; }
            /// <summary>
            /// The position of every first letters header on the page.
            /// </summary>
            [DataMember(Name = "headerPositions")]
            public object HeaderPositions { get; set; } // to do
            [DataMember(Name = "syncToken")]
            public string SyncToken { get; set; }
            /// <summary>
            /// The identifier for the accounts personal contact card.
            /// </summary>
            [DataMember(Name = "meCardId")]
            public string MeCardId { get; set; }
            /// <summary>
            /// Describes a list of groups users can be added to.
            /// </summary>
            [DataMember(Name = "groups")]
            public object[] Groups { get; set; } // to do
            [DataMember(Name = "prefToken")]
            public string PrefToken { get; set; }
            /// <summary>
            /// Describes a collection of groups that contacts can belong to.
            /// </summary>
            [DataMember(Name = "collections")]
            public iCloudContactsCollection[] Collections { get; set; }
            /// <summary>
            /// Describes a list of contacts the user has synchronized with iCloud.
            /// </summary>
            [DataMember(Name = "contacts")]
            public iCloudContactsContact[] Contacts { get; set; }
            /// <summary>
            /// Describes a list of contacts that have been removed.
            /// </summary>
            [DataMember(Name = "deletes")]
            public iCloudContactsDeletedCollection DeletedContacts { get; set; }

            /// <summary>
            /// Describes a collection of contacts which can be grouped together.
            /// </summary>
            [DataContract]
            public class iCloudContactsCollection
            {
                /// <summary>
                /// The order which the groups are ordered in.
                /// </summary>
                [DataMember(Name = "groupsOrder")]
                public object[] GroupsOrder { get; set; } // to do
                [DataMember(Name = "etag")]
                public string ETag { get; set; }
                /// <summary>
                /// An identifier for our collection.
                /// </summary>
                [DataMember(Name = "collectionId")]
                public string CollectionId { get; set; }
            }

            /// <summary>
            /// Describes a collection of contacts which have been removed.
            /// </summary>
            [DataContract]
            public class iCloudContactsDeletedCollection
            {
                /// <summary>
                /// Describes a list of contacts by ID that were removed.
                /// </summary>
                [DataMember(Name = "contactIds")]
                public string[] ContactIds { get; set; }
            }
        }

        /// <summary>
        /// This structure describes a request to update/add a set of contacts.
        /// </summary>
        [DataContract]
        public class iCloudContactsUpdateRequest
        {
            /// <summary>
            /// Describes a list of contacts the user wishes to synchronize with iCloud.
            /// </summary>
            [DataMember(Name = "contacts")]
            public iCloudContactsContact[] Contacts { get; set; }

            /// <summary>
            /// Our default constructor.
            /// </summary>
            /// <param name="contacts">An array of contacts that will be updated (or added if they do not exist)</param>
            public iCloudContactsUpdateRequest(iCloudContactsContact[] contacts)
            {
                Contacts = contacts;
            }
        }

        /// <summary>
        /// This structure describes a request to delete a set of contacts.
        /// </summary>
        [DataContract]
        public class iCloudContactsDeleteRequest
        {
            /// <summary>
            /// Describes a list of contacts the user wishes to synchronize with iCloud.
            /// </summary>
            [DataMember(Name = "contacts")]
            public iCloudContactsDeleteEntry[] Contacts { get; set; }

            /// <summary>
            /// This structure describes a contact we wish to delete.
            /// </summary>
            [DataContract]
            public class iCloudContactsDeleteEntry
            {
                /// <summary>
                /// Describes a unique identifier for the contact card.
                /// </summary>
                [DataMember(Name = "contactId", EmitDefaultValue = false)]
                public string Id { get; set; }
                [DataMember(Name = "etag", EmitDefaultValue = false)]
                public string ETag { get; set; }

                public iCloudContactsDeleteEntry() { }
                public iCloudContactsDeleteEntry(string id, string eTag)
                {
                    Id = id;
                    ETag = eTag;
                }
            }

            /// <summary>
            /// Our default constructor.
            /// </summary>
            /// <param name="contacts">An array of descriptors for contacts that will be deleted.</param>
            public iCloudContactsDeleteRequest(iCloudContactsDeleteEntry[] contacts)
            {
                Contacts = contacts;
            }
            /// <summary>
            /// Our default constructor.
            /// </summary>
            /// <param name="contacts">An array of contacts that will be deleted.</param>
            public iCloudContactsDeleteRequest(iCloudContactsContact[] contacts)
            {
                Contacts = new iCloudContactsDeleteEntry[contacts.Length];
                for (int i = 0; i < contacts.Length; i++)
                    Contacts[i] = new iCloudContactsDeleteEntry(contacts[i].Id, contacts[i].ETag);
            }
        }

        /// <summary>
        /// This structure describes a contact card for a contact the user has synchronized with iCloud servers.
        /// </summary>
        [DataContract]
        public class iCloudContactsContact
        {
            /// <summary>
            /// The middle name for the contact.
            /// </summary>
            [DataMember(Name = "middleName", EmitDefaultValue = false)]
            public string MiddleName { get; set; }
            /// <summary>
            /// The last name for the contact.
            /// </summary>
            [DataMember(Name = "lastName", EmitDefaultValue = false)]
            public string LastName { get; set; }
            /// <summary>
            /// A string representing the listed birthday for the contact.
            /// </summary>
            [DataMember(Name = "birthday", EmitDefaultValue = false)]
            public string Birthday { get; set; }
            [DataMember(Name = "etag", EmitDefaultValue = false)]
            public string ETag { get; set; }
            /// <summary>
            /// A set of Urls pointing to web pages or homepages associated with the contact.
            /// Because of older versions of iOS software and social networking software, networks
            /// such as twitter may have stored information here.
            /// </summary>
            [DataMember(Name = "urls", EmitDefaultValue = false)]
            public iCloudContactsStringEntry[] Urls { get; set; }
            /// <summary>
            /// A list of dates represented as strings for the contact, such as an anniversary.
            /// </summary>
            [DataMember(Name = "dates", EmitDefaultValue = false)]
            public iCloudContactsStringEntry[] Dates { get; set; }
            /// <summary>
            /// The department the contact is employed within.
            /// </summary>
            [DataMember(Name = "department", EmitDefaultValue = false)]
            public string Department { get; set; }
            /// <summary>
            /// A list of addresses provided for the contact.
            /// </summary>
            [DataMember(Name = "streetAddresses", EmitDefaultValue = false)]
            public iCloudContactsContactAddress[] StreetAddresses { get; set; }
            /// <summary>
            /// A boolean indicating whether the contact is a company, or whether it is an individual.
            /// </summary>
            [DataMember(Name = "isCompany", EmitDefaultValue = false)]
            public bool IsCompany { get; set; }
            /// <summary>
            /// A given suffix for the contact.
            /// </summary>
            [DataMember(Name = "suffix", EmitDefaultValue = false)]
            public string Suffix { get; set; }
            /// <summary>
            /// Describes a list of phone numbers provided for the contact.
            /// </summary>
            [DataMember(Name = "phones", EmitDefaultValue = false)]
            public iCloudContactsStringEntry[] Phones { get; set; }
            /// <summary>
            /// Describes a unique identifier for the contact card.
            /// </summary>
            [DataMember(Name = "contactId", EmitDefaultValue = false)]
            public string Id { get; set; }
            /// <summary>
            /// A nickname provided for the contact.
            /// </summary>
            [DataMember(Name = "nickName", EmitDefaultValue = false)]
            public string NickName { get; set; }
            /// <summary>
            /// A prefix provided for the contact.
            /// </summary>
            [DataMember(Name = "prefix", EmitDefaultValue = false)]
            public string Prefix { get; set; }
            /// <summary>
            /// The first name for the contact.
            /// </summary>
            [DataMember(Name = "firstName", EmitDefaultValue = false)]
            public string FirstName { get; set; }
            /// <summary>
            /// The provided phonetic first name for the contact.
            /// </summary>
            [DataMember(Name = "phoneticFirstName", EmitDefaultValue = false)]
            public string PhoneticFirstName { get; set; }
            /// <summary>
            /// The provided phonetic last name for the contact.
            /// </summary>
            [DataMember(Name = "phoneticLastName", EmitDefaultValue = false)]
            public string PhoneticLastName { get; set; }
            /// <summary>
            /// The company name provided for the contact. (the contact may be the company, or an individual which is employed there)
            /// </summary>
            [DataMember(Name = "companyName", EmitDefaultValue = false)]
            public string CompanyName { get; set; }
            /// <summary>
            /// The job title for the contact at the company they have provided.
            /// </summary>
            [DataMember(Name = "jobTitle", EmitDefaultValue = false)]
            public string JobTitle { get; set; }
            /// <summary>
            /// Related names to the contact, such as family members.
            /// </summary>
            [DataMember(Name = "relatedNames", EmitDefaultValue = false)]
            public iCloudContactsStringEntry[] RelatedNames { get; set; }
            /// <summary>
            /// A note section provided for the contact.
            /// </summary>
            [DataMember(Name = "notes", EmitDefaultValue = false)]
            public string Notes { get; set; }
            /// <summary>
            /// A set of instant messaging account aliases for the contact, such as Skype or AIM.
            /// </summary>
            [DataMember(Name = "IMs", EmitDefaultValue = false)]
            public iCloudContactsContactIMEntry[] IMs { get; set; }
            /// <summary>
            /// A set of profiles for various web services and social networks such as Twitter or Facebook.
            /// </summary>
            [DataMember(Name = "profiles", EmitDefaultValue = false)]
            public iCloudContactsContactProfileEntry[] Profiles { get; set; }

            /// <summary>
            /// This structure describes a contact's address.
            /// </summary>
            [DataContract]
            public class iCloudContactsContactAddress
            {
                /// <summary>
                /// The actual contact's address information structure.
                /// </summary>
                [DataMember(Name = "field", EmitDefaultValue = false)]
                public iCloudContactsContactAddressField Field { get; set; }

                /// <summary>
                /// A label for the contact, such as HOME or WORK.
                /// </summary>
                [DataMember(Name = "label", EmitDefaultValue = false)]
                public string Label { get; set; }

                /// <summary>
                /// This structure contains the underlying contact address information.
                /// </summary>
                [DataContract]
                public class iCloudContactsContactAddressField
                {
                    /// <summary>
                    /// The postal code provided for the contact.
                    /// </summary>
                    [DataMember(Name = "postalCode", EmitDefaultValue = false)]
                    public string PostalCode { get; set; }
                    /// <summary>
                    /// The street address provided for the contact.
                    /// </summary>
                    [DataMember(Name = "street", EmitDefaultValue = false)]
                    public string Street { get; set; }
                    /// <summary>
                    /// The 2-character country code provided for the contact.
                    /// </summary>
                    [DataMember(Name = "countryCode", EmitDefaultValue = false)]
                    public string CountryCode { get; set; }
                    /// <summary>
                    /// The state provided for the contact.
                    /// </summary>
                    [DataMember(Name = "state", EmitDefaultValue = false)]
                    public string State { get; set; }
                    /// <summary>
                    /// The city provided for the contact.
                    /// </summary>
                    [DataMember(Name = "city", EmitDefaultValue = false)]
                    public string City { get; set; }
                    /// <summary>
                    /// The name of the country provided for the contact.
                    /// </summary>
                    [DataMember(Name = "country", EmitDefaultValue = false)]
                    public string Country { get; set; }
                }
            }

            /// <summary>
            /// A structure describing an instant messaging account provided for the contact.
            /// </summary>
            [DataContract]
            public class iCloudContactsContactIMEntry
            {
                /// <summary>
                /// The actual instant messaging service information.
                /// </summary>
                [DataMember(Name = "field", EmitDefaultValue = false)]
                public iCloudContactsContactIMField Field { get; set; }

                /// <summary>
                /// A label describing the instant messaging service's prescence (HOME / WORK).
                /// </summary>
                [DataMember(Name = "label", EmitDefaultValue = false)]
                public string Label { get; set; }

                /// <summary>
                /// Contains information about the underlying IM service and provided details.
                /// </summary>
                [DataContract]
                public class iCloudContactsContactIMField
                {
                    /// <summary>
                    /// The username for the IM service.
                    /// </summary>
                    [DataMember(Name = "userName", EmitDefaultValue = false)]
                    public string Username { get; set; }
                    /// <summary>
                    /// The instant messaging service the username pertains to.
                    /// </summary>
                    [DataMember(Name = "IMService", EmitDefaultValue = false)]
                    public string IMService { get; set; }
                }
            }

            /// <summary>
            /// This structure describes a provided profile to a social network for the contact.
            /// </summary>
            [DataContract]
            public class iCloudContactsContactProfileEntry
            {
                /// <summary>
                /// A Url or other useful information for the social network account.
                /// </summary>
                [DataMember(Name = "field", EmitDefaultValue = false)]
                public string Field { get; set; }

                /// <summary>
                /// The social network the profile describes.
                /// </summary>
                [DataMember(Name = "label", EmitDefaultValue = false)]
                public string Label { get; set; }

                /// <summary>
                /// The username for the social network provided for the contact.
                /// </summary>
                [DataMember(Name = "user", EmitDefaultValue = false)]
                public string Username { get; set; }
            }

            /// <summary>
            /// Our default constructor, generates a new contact ID.
            /// </summary>
            public iCloudContactsContact()
            {
                // Generate a new ID.
                Id = Guid.NewGuid().ToString().ToUpper();
            }
        }


        /// <summary>
        /// A generic string entry describing an element in a list for the Contacts web service.
        /// </summary>
        [DataContract]
        public class iCloudContactsStringEntry
        {
            /// <summary>
            /// The actual value of the field.
            /// </summary>
            [DataMember(Name = "field")]
            public string Field { get; set; }

            /// <summary>
            /// The label describing the field.
            /// </summary>
            [DataMember(Name = "label")]
            public string Label { get; set; }

        }
        #endregion

        #endregion
    }
}
