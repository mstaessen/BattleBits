using System;
using System.Web.Mvc;
using BattleBits.Web.ViewModels;

namespace BattleBits.Web.Controllers
{
    public class CompetitionController : Controller
    {
        public ActionResult Index()
        {
            return RedirectToActionPermanent("Index", "Home");
        }

        public ActionResult Display(Guid id)
        {
            return View();
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