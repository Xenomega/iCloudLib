using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace iCloudLib
{
    /// <summary>
    /// A WebClient extension providing support for automatically setting cookies on web requests.
    /// </summary>
    internal class NetClient : WebClient
    {
        /// <summary>
        /// The actual cookie container to store all of the cookies we've set.
        /// </summary>
        public CookieContainer CookieContainer { get; private set; }

        /// <summary>
        /// Our default constructor.
        /// </summary>
        public NetClient()
        {
            CookieContainer = new CookieContainer();
            this.Encoding = System.Text.Encoding.UTF8;
        }
        /// <summary>
        /// Our constructor that quickly sets headers.
        /// </summary>
        /// <param name="origin">The page which we originated from.</param>
        /// <param name="referer">The page which brought us to this destination.</param>
        /// <param name="contentType">The content-type to be handled.</param>
        public NetClient(string origin, string referer, string contentType = "text/plain")
        {
            CookieContainer = new CookieContainer();
            this.Headers.Add("Origin", origin);
            this.Headers.Add("Referer", referer);
            this.Headers.Add("Content-Type", contentType);
            this.Encoding = System.Text.Encoding.UTF8;
        }
        /// <summary>
        /// Our constructor that allows for existing cookies.
        /// </summary>
        public NetClient(CookieContainer cookieContainer)
        {
            CookieContainer = cookieContainer;
            this.Encoding = System.Text.Encoding.UTF8;
        }
        /// <summary>
        /// Our constructor that allows for existing cookies and quickly sets headers.
        /// </summary>
        /// <param name="cookieContainer">The container holding all the cookies</param>
        /// <param name="origin">The page which we originated from.</param>
        /// <param name="referer">The page which brought us to this destination.</param>
        /// <param name="contentType">The content-type to be handled.</param>
        public NetClient(CookieContainer cookieContainer, string origin, string referer, string contentType = "text/plain")
        {
            CookieContainer = cookieContainer;
            this.Headers.Add("Origin", origin);
            this.Headers.Add("Referer", referer);
            this.Headers.Add("Content-Type", contentType);
            this.Encoding = System.Text.Encoding.UTF8;
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            // Get our web request as we would normally.
            WebRequest request = base.GetWebRequest(address);

            // But give it our cookie container, so it updates our cookies.
            if (request.GetType() == typeof(HttpWebRequest))
                ((HttpWebRequest)request).CookieContainer = CookieContainer;

            // And return it as the normal function does.
            return request;
        }

        public string GET(string url)
        {
            return this.DownloadString(url);
        }
        public string POST(string url, string data, bool formurlencoded=true)
        {
            if(formurlencoded)
                this.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
            return this.UploadString(url, data);
        }
    }
}
