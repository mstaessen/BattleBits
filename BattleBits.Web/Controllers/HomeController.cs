using System.Collections.Generic;
using System.Web.Mvc;
using BattleBits.Web.ViewModels;

namespace BattleBits.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var model = new HomeViewModel {
                Competitions = new List<CompetitionViewModel> {
                    new CompetitionViewModel {
                        Name = "Test"
                    }
                }
            };
            return View(model);
        }
    }
}
