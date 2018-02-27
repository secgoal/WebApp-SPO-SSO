using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Threading.Tasks;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;
namespace TodoListWebApp.Models
{
    public class SharePointOnlineRepository
    {/// <summary>
        /// Get the access token
        /// </summary>
        /// <param name="clientId">Client ID of the Web API app</param>
        /// <param name="appKey">Client secret for the Web API app</param>
        /// <param name="aadInstance">The login URL for AAD</param>
        /// <param name="tenant">Your tenant (eg kirke.onmicrosoft.com)</param>
        /// <param name="resource">The resource being accessed
        ///(eg., https://kirke.sharepoint.com)
        /// </param>
        /// <returns>string containing the access token</returns>
        public static async Task<string> GetAccessToken(
            string clientId,
            string appKey,
            string aadInstance,
            string tenant,
            string resource)
        {
            string accessToken = null;
            AuthenticationResult result = null;

            ClientCredential clientCred = new ClientCredential(clientId, appKey);
            string authHeader = HttpContext.Current.Request.Headers["Authorization"];

            string userAccessToken = authHeader.Substring(authHeader.LastIndexOf(' ')).Trim();
            UserAssertion userAssertion = new UserAssertion(userAccessToken);

            string authority = String.Format(CultureInfo.InvariantCulture, aadInstance, tenant);

            AuthenticationContext authContext = new AuthenticationContext(authority);

            result = await authContext.AcquireTokenAsync(resource, clientCred, userAssertion);
            accessToken = result.AccessToken;

            return accessToken;
        }

        /// <summary>
        /// Gets list items from a list named Announcements
        /// </summary>
        /// <param name="siteURL">The URL of the SharePoint site</param>
        /// <param name="accessToken">The access token</param>
        /// <returns>string containing response from SharePoint</returns>
        public static async Task<string> GetAnnouncements(
            string siteURL,
            string accessToken)
        {
            //
            // Call the O365 API and retrieve list items from a list named Announcements
            //
            string requestUrl = siteURL + "/_api/Web/Lists/GetByTitle('Announcements')/Items?$select=Title";

            HttpClient client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/atom+xml"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            HttpResponseMessage response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string responseString = await response.Content.ReadAsStringAsync();
                var jsonresult = JObject.Parse(await response.Content.ReadAsStringAsync());
                List<Announcement> li_Ann = new List<Announcement>();
            
                foreach (var item in jsonresult["title"])
                {
                    Announcement ann = new Announcement();
                    ann.Title = item["title"].ToString();
                    li_Ann.Add(ann);
                }
                return responseString;
            }

            // An unexpected error occurred calling the O365 API.  Return a null value.
            return (null);
        }

        /// <summary>
        /// Gets the form digest value, required for modifying
        /// data in SharePoint.  This is not needed for bearer authentication and
        /// can be safely removed in this scenario, but is left here for posterity.
        /// </summary>
        /// <param name="siteURL">The URL of the SharePoint site</param>
        /// <param name="accessToken">The access token</param>
        /// <returns>string containing the form digest</returns>
        private static async Task<string> GetFormDigest(
            string siteURL,
            string accessToken)
        {
            //Get the form digest value in order to write data
            HttpClient client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(
                  HttpMethod.Post, siteURL + "/_api/contextinfo");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            HttpResponseMessage response = await client.SendAsync(request);

            string responseString = await response.Content.ReadAsStringAsync();

            XNamespace d = "http://schemas.microsoft.com/ado/2007/08/dataservices";
            var root = XElement.Parse(responseString);
            var formDigestValue = root.Element(d + "FormDigestValue").Value;

            return formDigestValue;
        }

        /// <summary>
        /// Adds an announcement to a SharePoint list
        /// named Announcements
        /// </summary>
        /// <param name="title">The title of the announcement to add</param>
        /// <param name="siteURL">The URL of the SharePoint site</param>
        /// <param name="accessToken">The access token</param>
        /// <returns></returns>
        public static async Task<string> AddAnnouncement(
            string title,
            string siteURL,
            string accessToken)
        {
            //
            // Call the O365 API and retrieve the user's profile.
            //
            string requestUrl =
                siteURL + "/_api/Web/Lists/GetByTitle('Announcements')/Items";

            title = title.Replace('\'', ' ');
            //get the form digest, required for SharePoint list item modifications
            var formDigest = await GetFormDigest(siteURL, accessToken);

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add(
                "Accept",
                "application/json;odata=verbose");

            HttpRequestMessage request =
                new HttpRequestMessage(HttpMethod.Post, requestUrl);
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            // Note that the form digest is not needed for bearer authentication.  This can
            //safely be removed, but left here for posterity.            
            request.Headers.Add("X-RequestDigest", formDigest);

            var requestContent = new StringContent(
              "{ '__metadata': { 'type': 'SP.Data.AnnouncementsListItem' }, 'Title': '" + title + "'}");
            requestContent.Headers.ContentType =
               System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json;odata=verbose");

            request.Content = requestContent;

            HttpResponseMessage response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string responseString = await response.Content.ReadAsStringAsync();
                return responseString;
            }

            // An unexpected error occurred calling the O365 API.  Return a null value.
            return (null);
        }
    }
}