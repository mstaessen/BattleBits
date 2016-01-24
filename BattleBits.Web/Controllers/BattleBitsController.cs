using System.Web.Mvc;
using BattleBits.Web.ViewModels;

namespace BattleBits.Web.Controllers
{
    [Authorize]
    public class BattleBitsController : Controller
    {
        public ActionResult Display(int id)
        {
            return View(new BattleBitsViewModel {
                CompetitionId = id
            });
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