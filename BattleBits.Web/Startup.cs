using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(BattleBits.Web.Startup))]
namespace BattleBits.Web
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.ConfigureAuth();
            app.MapSignalR();
        }
    }
}
