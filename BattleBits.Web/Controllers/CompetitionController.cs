using System.Web.Mvc;
using BattleBits.Web.Models;
using BattleBits.Web.ViewModels;
using System.Linq;
using System;
using System.Data.Entity;

namespace BattleBits.Web.Controllers
{
    [Authorize]
    public class CompetitionController : Controller
    {
        public ActionResult Index()
        {
            using (var context = new CompetitionContext()) {
                var competitions = context.Competitions
                    .Include(x => x.Games);
                var model = new HomeViewModel {
                    Competitions = competitions.Select(c => new CompetitionViewModel {
                        Id = c.Id,
                        GameType = c.GameType,
                        Name = c.Name,
                        NumberOfGames = c.Games.Count
                    }).ToList()
                };
                return View(model);
            }
        }

        public ActionResult Display(int id)
        {
            using (var context = new CompetitionContext()) {
                var competition = context.Competitions.Find(id);
                var model = new CompetitionRankingViewModel {
                    Id = competition.Id,
                    Name = competition.Name,
                    Scores = context.Scores.Where(entry => entry.Game.Competition.Id == competition.Id).OrderByDescending(e => e.Value).ThenBy(e => e.Time).Take(50).ToList()
                };
                // TODO remove when actual records are added
                model.Scores.Add(new Score {
                    UserId = "mstaessen",
                    Value = 29,
                    Time = TimeSpan.FromSeconds(134)
                });
                model.Scores.Add(new Score {
                    UserId = "Samwise",
                    Value = 23,
                    Time = TimeSpan.FromSeconds(100)
                });

                return View(model);
            }
        }
    }
}