using BattleBits.Web.Models;
using System;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace BattleBits.Web
{
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // FOR NOW A COMPETITION FOR EACH GAMETYPE
//            using (var db = new CompetitionContext())
//            {
//                foreach(var type in Enum.GetValues(typeof(GameType)).Cast<GameType>())
//                {
//                    if(db.Competitions.All(c => c.GameType != type))
//                    {
//                        db.Competitions.Add(new Competition
//                        {
//                            Name = $"{type} @ 10 years visug",
//                            GameType = type
//                        });
//                    }
//                }
//                db.SaveChanges();
//            }
        }
    }
}
