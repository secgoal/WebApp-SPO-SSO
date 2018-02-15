---
services: active-directory
platforms: dotnet
author: Jerry
This project is developed based on an Azure Sample: active-directory-dotnet-webapp-webapi-openidconnect https://github.com/Azure-Samples/active-directory-dotnet-webapp-webapi-openidconnect. I added Single Sing on feature and use Graph API to get SPO items and display items on SPO page. 
---

# Calling a web API in a web app using Azure AD and OpenID Connect  

This sample shows how to build an MVC web application that uses Azure AD for sign-in using the OpenID Connect protocol, and then calls a Graph API under the signed-in user's identity using tokens obtained via OAuth 2.0. This sample uses the OpenID Connect ASP.Net OWIN middleware and ADAL .Net.

For more information about how the protocols work in this scenario and other scenarios, see [Authentication Scenarios for Azure AD](http://go.microsoft.com/fwlink/?LinkId=394414).

A sample Test case:
1.	On a domain-joined client machine, the logged in user is contoso\o365
2.	He launch ASP.net MVC web application https://localhost:44328 and he doesn’t need input user name/password and automatically log-in ASP.net web application and with Office365 account o365@jerrylab001.onmicrosoft.com 
Because I set up ADFS for Office365 for Single Sign-On and enable auto-acceleration for my SharePoint Online tenancy, when he directly access to SharePoint Online site https://jerrylab001.sharepoint.com it will not show Office 365 login page and the user can immediately log on with local domain account. 
3.	On this site, I create a page called “SPO” which can display items under a document library https://jerrylab001.sharepoint.com/DocLib. The items are dynamically displayed based on logged in user. For example, o365 account has access to view 1 item “dc.docx” in that document library. 
But another account ake can view 2 items. So if on a client machine which is logged in by contoso\ake, then launch the ASP.net web application, that SPO page will display two items. 


## How To Run This Sample

To run this sample you will need:
- Visual Studio 2013
- An Internet connection
- An Azure Active Directory (Azure AD) tenant. For more information on how to get an Azure AD tenant, please see [How to get an Azure AD tenant](https://azure.microsoft.com/en-us/documentation/articles/active-directory-howto-tenant/) 
- A user account in your Azure AD tenant. This sample will not work with a Microsoft account, so if you signed in to the Azure portal with a Microsoft account and have never created a user account in your directory before, you need to do that now.
- Set up ADFS for Office365 for Single Sign-On. 
- Enable auto-acceleration for my SharePoint Online tenancy.
Please refer more details at Enable or disable auto-acceleration for your SharePoint Online tenancy https://support.office.com/en-us/article/enable-or-disable-auto-acceleration-for-your-sharepoint-online-tenancy-74985ebf-39e1-4c59-a74a-dcdfd678ef83 

### Step 1:  Clone or download this repository

### Step 2:  Register the sample with your Azure Active Directory tenant

Register this project in your Azure AD tenant.

#### Register the TodoListWebApp web app

1. Sign in to the [Azure portal](https://portal.azure.com).
2. On the top bar, click on your account and under the **Directory** list, choose the Active Directory tenant where you wish to register your application.
2. Click on **More Services** in the left hand nav, and choose **Azure Active Directory**.
3. Click on **App registrations** and choose **Add**.
4. Enter a friendly name for the application, for example 'TodoListWebApp' and select 'Web Application and/or Web API' as the Application Type. For the sign-on URL, enter the base URL for the sample, which is by default `https://localhost:44322`. NOTE:  It is important, due to the way Azure AD matches URLs, to ensure there is a trailing slash on the end of this URL.  If you don't include the trailing slash, you will receive an error when the application attempts to redeem an authorization code. Click on **Create** to create the application. 
5. While still in the Azure portal, choose your application, click on **Settings** and choose **Properties**.
6. Find the Application ID value and copy it to the clipboard.
7. On the same page, change the `Logout Url` property to `https://localhost:44322/Account/EndSession`.  This is the default single sign out URL for this sample. 
7. From the Settings menu, choose **Keys** and add a key - select a key duration of either 1 year or 2 years. When you save this page, the key value will be displayed, copy and save the value in a safe location - you will need this key later to configure the project in Visual Studio - this key value will not be displayed again, nor retrievable by any other means, so please record it as soon as it is visible from the Azure Portal.
8. Configure Permissions for your application - in the Settings menu, choose the 'Required permissions' section, click on **Add**, then **Select an API**, and type 'TodoListService' in the textbox. Then, click on  **Select Permissions** and select 'Access TodoListService'.

### Step 3:  Configure the sample to use your Azure AD tenant

#### Configure the TodoListWebApp project

1. Open the solution in Visual Studio 2013.
2. Open the `web.config` file.
3. Find the app key `ida:Tenant` and replace the value with your AAD tenant name.
4. Find the app key `ida:ClientId` and replace the value with the Application ID for the TodoListWebApp from the Azure portal.
5. Find the app key `ida:AppKey` and replace the value with the key for the TodoListWebApp from the Azure portal.
6. If you changed the base URL of the TodoListWebApp sample, find the app key `ida:PostLogoutRedirectUri` and replace the value with the new base URL of the sample.
7. Find the app key `todo:TodoListBaseAdress` ane make sure it has the correct value for the address of the TodoListService project.
8. Find the app key `todo:TodoListResourceId` and replace the value with the App ID URI registered for the TodoListService.

### Step 4:  Trust the IIS Express SSL certificate

Since the web API is SSL protected, the client of the API (the web app) will refuse the SSL connection to the web API unless it trusts the API's SSL certificate.  Use the following steps in Windows Powershell to trust the IIS Express SSL certificate.  You only need to do this once.  If you fail to do this step, calls to the TodoListService will always throw an unhandled exception where the inner exception message is:

"The underlying connection was closed: Could not establish trust relationship for the SSL/TLS secure channel."

To configure your computer to trust the IIS Express SSL certificate, begin by opening a Windows Powershell command window as Administrator.

Query your personal certificate store to find the thumbprint of the certificate for `CN=localhost`:

```
PS C:\windows\system32> dir Cert:\LocalMachine\My


    Directory: Microsoft.PowerShell.Security\Certificate::LocalMachine\My


Thumbprint                                Subject
----------                                -------
C24798908DA71693C1053F42A462327543B38042  CN=localhost
```

Next, add the certificate to the Trusted Root store:

```
PS C:\windows\system32> $cert = (get-item cert:\LocalMachine\My\C24798908DA71693C1053F42A462327543B38042)
PS C:\windows\system32> $store = (get-item cert:\Localmachine\Root)
PS C:\windows\system32> $store.Open("ReadWrite")
PS C:\windows\system32> $store.Add($cert)
PS C:\windows\system32> $store.Close()
```

You can verify the certificate is in the Trusted Root store by running this command:

`PS C:\windows\system32> dir Cert:\LocalMachine\Root`

### Step 5:  Run the sample

Clean the solution, rebuild the solution, and run it.  You might want to go into the solution properties and set both projects as startup projects, with the service project starting first.

1.	On a domain-joined client machine, the logged in user is contoso\o365
2.	He launch ASP.net MVC web application https://localhost:44328 and he doesn’t need input user name/password and automatically log-in ASP.net web application and with Office365 account o365@jerrylab001.onmicrosoft.com 
Because I set up ADFS for Office365 for Single Sign-On and enable auto-acceleration for my SharePoint Online tenancy, when he directly access to SharePoint Online site https://jerrylab001.sharepoint.com it will not show Office 365 login page and the user can immediately log on with local domain account. 
3.	On this site, I create a page called “SPO” which can display items under a document library https://jerrylab001.sharepoint.com/DocLib. The items are dynamically displayed based on logged in user. For example, o365 account has access to view 1 item “dc.docx” in that document library. 
But another account ake can view 2 items. So if on a client machine which is logged in by contoso\ake, then launch the ASP.net web application, that SPO page will display two items. 

## How To Recreate This Sample

First, in Visual Studio 2013 create an empty solution to host the  projects.  Then, follow these steps to create each project.

### Creating the TodoListWebApp Project

1. In the solution, create a new ASP.Net MVC web application called TodoListWebApp with Authentication set to No Authentication.
2. Set SSL Enabled to be True.  Note the SSL URL.
3. In the project properties, Web properties, set the Project Url to be the SSL URL.
4. Add the following ASP.Net OWIN middleware NuGets: Microsoft.IdentityModel.Protocol.Extensions, System.IdentityModel.Tokens.Jwt, Microsoft.Owin.Security.OpenIdConnect, Microsoft.Owin.Security.Cookies, Microsoft.Owin.Host.SystemWeb.
5. Add the Active Directory Authentication Library NuGet (`Microsoft.IdentityModel.Clients.ActiveDirectory`).
6. In the `App_Start` folder, create a class `Startup.Auth.cs`.  You will need to remove `.App_Start` from the namespace name.  Replace the code for the `Startup` class with the code from the same file of the sample app.  Be sure to take the whole class definition!  The definition changes from `public class Startup` to `public partial class Startup`.
7. Right-click on the project, select Add,  select "OWIN Startup class", and name the class "Startup".  If "OWIN Startup Class" doesn't appear in the menu, instead select "Class", and in the search box enter "OWIN".  "OWIN Startup class" will appear as a selection; select it, and name the class `Startup.cs`.
8. In `Startup.cs`, replace the code for the `Startup` class with the code from the same file of the sample app.  Again, note the definition changes from `public class Startup` to `public partial class Startup`.
9. In the `Views` --> `Shared` folder, create a new partial view `_LoginPartial.cshtml`.  Replace the contents of the file with the contents of the file of same name from the sample.
10. In the `Views` --> `Shared` folder, replace the contents of `_Layout.cshtml` with the contents of the file of same name from the sample.  Effectively, all this will do is add a single line, `@Html.Partial("_LoginPartial")`, that lights up the previously added `_LoginPartial` view.
11. Create a new empty controller called `AccountController`.  Replace the implementation with the contents of the file of same name from the sample.
12. If you want the user to be required to sign-in before they can see any page of the app, then in the `HomeController`, decorate the `HomeController` class with the `[Authorize]` attribute.  If you leave this out, the user will be able to see the home page of the app without having to sign-in first, and can click the sign-in link on that page to get signed in.
13. In the `Models` folder add a new class called `TodoItem.cs`.  Copy the implementation of TodoItem from this sample into the class.
14. In the `Models` folder add a new class called `UserProfile.cs`.  Copy the implementation of UserProfile from this sample into the class.
15. In the project, create a new folder called `Utils`.  In the folder, create a new class called `NaiveSessionCache.cs`.  Copy the implementation of the class from the sample.
16. Add a new empty MVC5 controller TodoListController to the project.  Copy the implementation of the controller from the sample.  Remember to include the [Authorize] attribute on the class definition.
17. Add a new empty MVC5 controller UserProfileController to the project.  Copy the implementation of the controller from the sample.  Again, remember to include the [Authorize] attribute on the class definition.
18. In `Views` --> `TodoList` create a new view, `Index.cshtml`, and copy the implementation from this sample.
19. In `Views` --> `UserProfile` create a new view, `Index.cshtml`, and copy the implementation from this sample.
20. In the shared `_Layout` view, make sure the Action Links for Profile and To Do List that are in the sample have been added.
21. In `web.config`, in `<appSettings>`, create keys for `ida:ClientId`, `ida:AppKey`, `ida:AADInstance`, `ida:Tenant`, `ida:PostLogoutRedirectUri`, `ida:GraphResourceId`, and `ida:GraphUserUrl` and set the values accordingly.  For the public Azure AD, the value of `ida:AADInstance` is `https://login.microsoftonline.com/{0}` the value of `ida:GraphResourceId` is `https://graph.windows.net`, and the value of `ida:GraphUserUrl` is `https://graph.windows.net/{0}/me?api-version=2013-11-08`.
22. In `web.config` in `<appSettings>`, create keys for `todo:TodoListResourceId` and `todo:TodoListBaseAddress` and set the values accordinly.
23. In `web.config` add this line in the `<system.web>` section: `<sessionState timeout="525600" />`.  This increases the ASP.Net session state timeout to it's maximum value so that access tokens and refresh tokens cache in session state aren't cleared after the default timeout of 20 minutes.

Finally, in the properties of the solution itself, set both projects as startup projects.
