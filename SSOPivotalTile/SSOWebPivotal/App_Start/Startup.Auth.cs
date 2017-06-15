using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Owin;
using Pivotal.Configuration.CloudFoundry;
using Pivotal.Owin.Security.OpenIDConnect;
using System;
using System.Configuration;
using System.Net;

namespace SSOWebPivotal
{
    public partial class Startup
    {
        private static string clientId = ConfigurationManager.AppSettings["ida:ClientId"];
        private static string clientSecret = ConfigurationManager.AppSettings["ida:ClientSecret"];
        private static string redirectUri = ConfigurationManager.AppSettings["ida:RedirectUri"];
        private static string authority = ConfigurationManager.AppSettings["ida:Domain"];
        private static int appPort = 44316;
        private static string appHost = "localhost";

        public void ConfigureAuth(IAppBuilder app)
        {
            ServicePointManager.ServerCertificateValidationCallback +=

                (sender, cert, chain, sslPolicyErrors) => true;

            Pivotal.Configuration.CloudFoundry.ConfigurationBuilder config = Pivotal.Configuration.CloudFoundry.ConfigurationBuilder.Instance();

            Service svc = config.GetServiceByName("Internal-SSO");

            if (svc != null)
            {
                clientId = svc.Credentials["client_id"];
                clientSecret = svc.Credentials["client_secret"];
                authority = svc.Credentials["auth_domain"];
                appHost = System.Environment.GetEnvironmentVariable("VCAP_APPLICATION") == null ? "localhost" : redirectUri;
                appPort = System.Environment.GetEnvironmentVariable("VCAP_APPLICATION") == null ? 44316 : 0;
            }

            app.SetDefaultSignInAsAuthenticationType("ExternalCookie");
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = "ExternalCookie",
                AuthenticationMode = AuthenticationMode.Active,
                CookieName = ".AspNet.ExternalCookie",
                LoginPath = new PathString("/Home/AuthorizeSSO"),
                ExpireTimeSpan = TimeSpan.FromMinutes(5)
            });

            app.UseOpenIDConnect(new OpenIDConnectOptions()
            {
                ClientID = clientId,
                ClientSecret = clientSecret,
                AuthDomain = authority,
                AppHost = appHost,
                AppPort = appPort
                
            });
        }
    }
}
