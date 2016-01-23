using System.Web.Mvc;

namespace BattleBits.Web.Controllers
{
    public class BattleBitsController : Controller
    {
        public ActionResult Display(int id)
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