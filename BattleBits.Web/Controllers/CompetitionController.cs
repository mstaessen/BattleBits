using System.Collections.Generic;
using System.Web.Mvc;
using BattleBits.Web.Models;
using BattleBits.Web.ViewModels;

namespace BattleBits.Web.Controllers
{
    public class CompetitionController : Controller
    {

        public ActionResult Index()
        {
            using (var context = new CompetitionContext()) {
                var model = new HomeViewModel {
                    Competitions = new List<CompetitionViewModel> {
                        new CompetitionViewModel {
                            Name = "Test",
                            Url = Url.Action("Display", "BattleBits")
                        }
                    }
                };
                return View(model);
            }
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(CreateCompetitionViewModel model)
        {
            return View();
        }
    }
}