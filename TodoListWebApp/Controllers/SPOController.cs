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
    public class SPOController : Controller
    {
        private string graphResourceId = ConfigurationManager.AppSettings["ida:GraphResourceId"];
        private string graphUserUrl = ConfigurationManager.AppSettings["ida:GraphUserUrl"];
        private const string TenantIdClaimType = "http://schemas.microsoft.com/identity/claims/tenantid";
        private static string clientId = ConfigurationManager.AppSettings["ida:ClientId"];
        private static string appKey = ConfigurationManager.AppSettings["ida:AppKey"];
        public async Task<ActionResult> Index()
        {
            //
            // Retrieve the user's name, tenantID, and access token since they are parameters used to query the Graph API.
            //
            AuthenticationResult result = null;
            List<SPOList> li = new List<SPOList>();
            try
            {
                string tenantId = ClaimsPrincipal.Current.FindFirst(TenantIdClaimType).Value;
                string userObjectID = ClaimsPrincipal.Current.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;
                AuthenticationContext authContext = new AuthenticationContext(Startup.Authority, new NaiveSessionCache(userObjectID));
                ClientCredential credential = new ClientCredential(clientId, appKey);
                result = await authContext.AcquireTokenSilentAsync("https://graph.microsoft.com", credential, new UserIdentifier(userObjectID, UserIdentifierType.UniqueId));
                var url = "https://graph.microsoft.com/v1.0/sites/root/lists/doclib/items";
                string responseContent = String.Empty;

                using (HttpClient client = new HttpClient())
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, url);
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
                    HttpResponseMessage response = await client.SendAsync(request);
                    if (response.IsSuccessStatusCode)
                    {
                        responseContent = response.Content.ReadAsStringAsync().Result;
                        string responseString = await response.Content.ReadAsStringAsync();
                        //spoList = JsonConvert.DeserializeObject<SPOList>(responseString);
                        //IEnumerable<SPOList> list = JsonConvert.DeserializeObject<IEnumerable<SPOList>>(responseString);
                        var jsonresult = JObject.Parse(await response.Content.ReadAsStringAsync());

                        foreach (var item in jsonresult["value"])
                        {
                            SPOList spoList = new SPOList();
                            spoList.Name = item["webUrl"].ToString();
                            spoList.user = item["lastModifiedBy"]["user"]["displayName"].ToString();
                            li.Add(spoList);
                        }

                    }
                    else
                    {
                        //
                        // If the call failed, then drop the current access token and show the user an error indicating they might need to sign-in again.
                        //
                        var todoTokens = authContext.TokenCache.ReadItems().Where(a => a.Resource == graphResourceId);
                        foreach (TokenCacheItem tci in todoTokens)
                            authContext.TokenCache.DeleteItem(tci);
                        SPOList spoList = new SPOList();
                        spoList.displayName = "";
                        spoList.Name = "";
                        spoList.createdDateTime = "";
                        ViewBag.ErrorMessage = "UnexpectedError";
                    }
                }

                return View(li);
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
                SPOList spoList = new SPOList();
                spoList = new SPOList();
                spoList.displayName = "";
                spoList.Name = "";
                spoList.createdDateTime = "";

                ViewBag.ErrorMessage = "AuthorizationRequired";

                return View(spoList);

            }
        }
    }
}