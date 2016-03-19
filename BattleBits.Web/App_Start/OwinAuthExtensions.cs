using System;
using System.Configuration;
using System.Net.Http;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Google;
using Owin;
using BattleBits.Web.Models;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security.Facebook;
using Microsoft.Owin.Security.MicrosoftAccount;
using Microsoft.Owin.Security.Twitter;
using Owin.Security.Providers.GitHub;
using Owin.Security.Providers.LinkedIn;

namespace BattleBits.Web
{
    public static class OwinAuthExtensions
    {
        // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
        public static void ConfigureAuth(this IAppBuilder app)
        {
            var appSettings = ConfigurationManager.AppSettings;

            // Configure the db context, user manager and signin manager to use a single instance per request
            app.CreatePerOwinContext(CompetitionContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);

            // Enable the application to use a cookie to store information for the signed in user
            // and to use a cookie to temporarily store information about a user logging in with a third party login provider
            // Configure the sign in cookie
            app.UseCookieAuthentication(new CookieAuthenticationOptions {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login"),
                Provider = new CookieAuthenticationProvider {
                    // Enables the application to validate the security stamp when the user logs in.
                    // This is a security feature which is used when you change a password or add an external login to your account.  
                    OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, ApplicationUser>(
                        validateInterval: TimeSpan.FromMinutes(30),
                        regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync(manager))
                }
            });
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            // Enables the application to temporarily store user information when they are verifying the second factor in the two-factor authentication process.
            app.UseTwoFactorSignInCookie(DefaultAuthenticationTypes.TwoFactorCookie, TimeSpan.FromMinutes(5));

            // Enables the application to remember the second login verification factor such as phone or email.
            // Once you check this option, your second step of verification during the login process will be remembered on the device where you logged in from.
            // This is similar to the RememberMe option when you log in.
            app.UseTwoFactorRememberBrowserCookie(DefaultAuthenticationTypes.TwoFactorRememberBrowserCookie);

            if (!String.IsNullOrEmpty(appSettings["FacebookAppId"]) && !String.IsNullOrEmpty(appSettings["FacebookAppSecret"])) {
                var options = new FacebookAuthenticationOptions {
                    AppId = appSettings["FacebookAppId"],
                    AppSecret = appSettings["FacebookAppSecret"],
                    //http://stackoverflow.com/questions/32059384/why-new-fb-api-2-4-returns-null-email-on-mvc-5-with-identity-and-oauth-2
                    BackchannelHttpHandler = new FacebookBackChannelHandler(),
                    UserInformationEndpoint = "https://graph.facebook.com/v2.5/me?fields=id,name,email,first_name,last_name,location"
                };
                options.Scope.Add("email");
                app.UseFacebookAuthentication(options);
            }

            if (!String.IsNullOrEmpty(appSettings["TwitterConsumerKey"]) && !String.IsNullOrEmpty(appSettings["TwitterConsumerSecret"])) {
                var options = new TwitterAuthenticationOptions {
                    ConsumerKey = appSettings["TwitterConsumerKey"],
                    ConsumerSecret = appSettings["TwitterConsumerSecret"],
                    // http://stackoverflow.com/questions/25011890/owin-twitter-login-the-remote-certificate-is-invalid-according-to-the-validati
                    BackchannelCertificateValidator = new Microsoft.Owin.Security.CertificateSubjectKeyIdentifierValidator(new[] {
                        "A5EF0B11CEC04103A34A659048B21CE0572D7D47", // VeriSign Class 3 Secure Server CA - G2
                        "0D445C165344C1827E1D20AB25F40163D8BE79A5", // VeriSign Class 3 Secure Server CA - G3
                        "7FD365A7C2DDECBBF03009F34339FA02AF333133", // VeriSign Class 3 Public Primary Certification Authority - G5
                        "39A55D933676616E73A761DFA16A7E59CDE66FAD", // Symantec Class 3 Secure Server CA - G4
                        "‎add53f6680fe66e383cbac3e60922e3b4c412bed", // Symantec Class 3 EV SSL CA - G3
                        "4eb6d578499b1ccf5f581ead56be3d9b6744a5e5", // VeriSign Class 3 Primary CA - G5
                        "5168FF90AF0207753CCCD9656462A212B859723B", // DigiCert SHA2 High Assurance Server C‎A 
                        "B13EC36903F8BF4701D498261A0802EF63642BC3" // DigiCert High Assurance EV Root CA
                    })
                };
                app.UseTwitterAuthentication(options);
            }

            if (!String.IsNullOrEmpty(appSettings["GoogleClientId"]) && !String.IsNullOrEmpty(appSettings["GoogleClientSecret"])) {
                var options = new GoogleOAuth2AuthenticationOptions {
                    ClientId = appSettings["GoogleClientId"],
                    ClientSecret = appSettings["GoogleClientSecret"]
                };
                options.Scope.Add("https://www.googleapis.com/auth/userinfo.email");
                app.UseGoogleAuthentication(options);
            }

            if (!String.IsNullOrEmpty(appSettings["MicrosoftClientId"]) && !String.IsNullOrEmpty(appSettings["MicrosoftClientSecret"])) {
                app.UseMicrosoftAccountAuthentication(new MicrosoftAccountAuthenticationOptions {
                    ClientId = appSettings["MicrosoftClientId"],
                    ClientSecret = appSettings["MicrosoftClientSecret"]
                });
            }

            if (!String.IsNullOrEmpty(appSettings["GitHubClientId"]) && !String.IsNullOrEmpty(appSettings["GitHubClientSecret"])) {
                var githubOptions = new GitHubAuthenticationOptions {
                    ClientId = appSettings["GitHubClientId"],
                    ClientSecret = appSettings["GitHubClientSecret"]
                };
                githubOptions.Scope.Clear();
                githubOptions.Scope.Add("user:email");
                app.UseGitHubAuthentication(githubOptions);
            }

            if (!String.IsNullOrEmpty(appSettings["LinkedInClientId"]) && !String.IsNullOrEmpty(appSettings["LinkedInClientSecret"])) {
                app.UseLinkedInAuthentication(new LinkedInAuthenticationOptions {
                    ClientId = appSettings["LinkedInClientId"],
                    ClientSecret = appSettings["LinkedInClientSecret"]
                });
            }
        }
    }

    public class FacebookBackChannelHandler : HttpClientHandler
    {
        protected override async System.Threading.Tasks.Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            // Replace the RequestUri so it's not malformed
            if (!request.RequestUri.AbsolutePath.Contains("/oauth")) {
                request.RequestUri = new Uri(request.RequestUri.AbsoluteUri.Replace("?access_token", "&access_token"));
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}