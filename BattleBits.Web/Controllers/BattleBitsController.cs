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

        public ActionResult Practice() => View();

        public ActionResult LeaderboardTemplate() => View();

        public ActionResult GameDisplayTemplate() => View();

        public ActionResult GamePlayTemplate() => View();
    }
}