using System;
using System.Web.Mvc;

namespace BattleBits.Web.Controllers
{
    public class BattleBitsController : Controller
    {
        public ActionResult Display(Guid id)
        {
            return View();
        }

        public ActionResult LeaderboardTemplate()
        {
            return View();
        }

        public ActionResult GameDisplayTemplate()
        {
            return View();
        }

        public ActionResult GamePlayTemplate()
        {
            return View();

        }
    }
}