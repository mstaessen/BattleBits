using System;
using System.Security.Policy;
using System.Web;
using System.Web.Mvc;
using Microsoft.SqlServer.Server;

namespace BattleBits.Web.Helpers
{
    public static class UrlHelperExtensions
    {
        public static string Content(this UrlHelper helper, string contentPath, bool toAbsolute)
        {
            var path = helper.Content(contentPath);
            var url = new Uri(HttpContext.Current.Request.Url, path);
            return toAbsolute ? url.AbsoluteUri : path;
        }
    }
}