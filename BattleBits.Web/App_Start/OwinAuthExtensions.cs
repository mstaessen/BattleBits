using System;
using System.Configuration;
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

            // Uncomment the following lines to enable logging in with third party login providers
//            app.UseFacebookAuthentication(new FacebookAuthenticationOptions {
//                AppId = appSettings["FacebookAppId"],
//                AppSecret = appSettings["FacebookAppSecret"]
//            });
//            app.UseTwitterAuthentication(new TwitterAuthenticationOptions {
//                ConsumerKey = appSettings["TwitterConsumerKey"],
//                ConsumerSecret = appSettings["TwitterConsumerSecret"]
//            });
            app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions {
                ClientId = appSettings["GoogleClientId"],
                ClientSecret = appSettings["GoogleClientSecret"]
            });
            app.UseMicrosoftAccountAuthentication(new MicrosoftAccountAuthenticationOptions {
                ClientId = appSettings["MicrosoftClientId"],
                ClientSecret = appSettings["MicrosoftClientSecret"]
            });

            var githubOptions = new GitHubAuthenticationOptions {
                ClientId = appSettings["GitHubClientId"],
                ClientSecret = appSettings["GitHubClientSecret"],
            };
            githubOptions.Scope.Clear();
            githubOptions.Scope.Add("user:email");
            app.UseGitHubAuthentication(githubOptions);

            app.UseLinkedInAuthentication(new LinkedInAuthenticationOptions {
                ClientId = appSettings["LinkedInClientId"],
                ClientSecret = appSettings["LinkedInClientSecret"]
            });
        }
    }
}