using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
// The following using statements were added for this sample.
using System.Threading.Tasks;
using TodoListWebApp.Models;
using System.Security.Claims;
using Microsoft.Owin.Security.OpenIdConnect;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using TodoListWebApp.Utils;
using System.Configuration;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Owin.Security;
using System.Net;
using Newtonsoft.Json.Linq;
namespace TodoListWebApp.Controllers
{
    [Authorize]
    public class SPOAPIController : Controller
    {
        private const string TenantIdClaimType = "http://schemas.microsoft.com/identity/claims/tenantid";
        private static string clientId = ConfigurationManager.AppSettings["ida:ClientId"];
        private static string appKey = ConfigurationManager.AppSettings["ida:AppKey"];
        public async Task<ActionResult> Index()
        {
            string resource = "https://jerrylab001.sharepoint.com";

            AuthenticationResult result = null;
            List<Announcement> li_announcement = new List<Announcement>();
            try
            {
                string tenantId = ClaimsPrincipal.Current.FindFirst(TenantIdClaimType).Value;
                string userObjectID = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;
                AuthenticationContext authContext = new AuthenticationContext(Startup.Authority, new NaiveSessionCache(userObjectID));
                ClientCredential credential = new ClientCredential(clientId, appKey);
                result = await authContext.AcquireTokenSilentAsync(resource, credential, new UserIdentifier(userObjectID, UserIdentifierType.UniqueId));
                string requestUrl = resource + "/_api/Web/Lists/GetByTitle('Announcement')/Items?$select=Title";
                string responseContent = String.Empty;
                string responseString = String.Empty;

                using (HttpClient client = new HttpClient())
                {
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
                    HttpResponseMessage response = await client.SendAsync(request);
                    if (response.IsSuccessStatusCode)
                    {
                        responseString = await response.Content.ReadAsStringAsync();
                        var jsonresult = JObject.Parse(await response.Content.ReadAsStringAsync());
                        foreach (var item in jsonresult["value"])
                        {
                            Announcement ann = new Announcement();
                            ann.Title = item["Title"].ToString();
                            li_announcement.Add(ann);
                        }
                    }
                }
               
                return View(li_announcement);
            }
            catch (AdalException)
            {
                //
                // If the user doesn't have an access token, they need to re-authorize.
                //

                //
                // If refresh is set to true, the user has clicked the link to be authorized again.
                //
                if (Request.QueryString["reauth"] == "True")
                {
                    //
                    // Send an OpenID Connect sign-in request to get a new set of tokens.
                    // If the user still has a valid session with Azure AD, they will not be prompted for their credentials.
                    // The OpenID Connect middleware will return to this controller after the sign-in response has been handled.
                    //
                    HttpContext.GetOwinContext().Authentication.Challenge(
                        new AuthenticationProperties(),
                        OpenIdConnectAuthenticationDefaults.AuthenticationType);
                }

                //
                // The user needs to re-authorize.  Show them a message to that effect.
                //
                Announcement ann = new Announcement();
                ann.Title = "";

                ViewBag.ErrorMessage = "AuthorizationRequired";

                return View(ann);

            }
        }
        
    }
}