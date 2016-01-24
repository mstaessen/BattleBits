using System.Web.Mvc;
using BattleBits.Web.Models;
using BattleBits.Web.ViewModels;
using System.Linq;
using System;

namespace BattleBits.Web.Controllers
{
    public class CompetitionController : Controller
    {

        public ActionResult Index()
        {
            using (var context = new CompetitionContext()) {
                var gamesPlayed = context.Competitions.GroupBy(entry => entry.Id).ToDictionary(group => group.Key, group => group.Count());
                var competitions = context.Competitions.ToList().Select(
                    c => new CompetitionViewModel {
                        Id = c.Id,
                        GameType = c.GameType,
                        Name = c.Name,
                        NumberOfGames = gamesPlayed.ContainsKey(c.Id) ? gamesPlayed[c.Id] : 0
                    }).ToList();
                var model = new HomeViewModel {
                    Competitions = competitions
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
                    Scores = context.Scores.Where(entry => entry.Game.Competition.Id == competition.Id).OrderByDescending(e => e.Value).ThenBy(e => e.Duration).Take(50).ToList()
                };
                // TODO remove when actual records are added
                model.Scores.Add(new Score {
                    UserId = "mstaessen",
                    Value = 29,
                    Duration = TimeSpan.FromSeconds(134)
                });
                model.Scores.Add(new Score {
                    UserId = "Samwise",
                    Value = 23,
                    Duration = TimeSpan.FromSeconds(100)
                });

                return View(model);
            }
        }
    }
}