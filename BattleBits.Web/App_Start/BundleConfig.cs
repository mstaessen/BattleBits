using System.Web.Optimization;

namespace BattleBits.Web
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new StyleBundle("~/Content/bootstrap").Include("~/Content/bootstrap.css"));
            bundles.Add(new StyleBundle("~/Content/battle-bits").Include("~/Content/battle-bits.css"));
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include("~/Scripts/jquery.min.js"));
            bundles.Add(new ScriptBundle("~/bundles/lodash").Include("~/Scripts/lodash.min.js"));
            bundles.Add(new ScriptBundle("~/bundles/signalr").Include("~/Scripts/jquery.signalr.min.js"));
            bundles.Add(new ScriptBundle("~/bundles/angular").Include("~/Scripts/angular.min.js", "~/Scripts/angular-route.min.js"));
            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include("~/Scripts/bootstrap.min.js"));
            bundles.Add(new ScriptBundle("~/bundles/battle-bits").Include("~/Scripts/battle-bits.js"));
        }
    }
}
