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
            using (var context = new CompetitionContext())
            {
                var gamesPlayed = context.GameEntries.GroupBy(entry => entry.Competition).ToDictionary(group => group.Key.Id, group => group.Count());
                var competitions = context.Competitions.ToList().Select(
                    c => new CompetitionViewModel
                    {
                        Id = c.Id,
                        GameType = c.GameType,
                        Name = c.Name,
                        NumberOfGames = gamesPlayed.ContainsKey(c.Id) ? gamesPlayed[c.Id] : 0
                    }).ToList();
                var model = new HomeViewModel
                {
                    Competitions = competitions
                };
                return View(model);
            };
        }

        public ActionResult Display(int id)
        {
            using (var context = new CompetitionContext())
            {
                var competition = context.Competitions.Find(id);
                var model = new CompetitionRankingViewModel
                {
                    Id = competition.Id,
                    Name = competition.Name,
                    GameEntries = context.GameEntries.Where(entry => entry.Competition.Id == competition.Id).OrderByDescending(e => e.Score).ThenBy(e => e.Duration).Take(50).ToList()
                };
                // TODO remove when actual records are added
                model.GameEntries.Add(new GameEntry
                {
                    User = new User { Name = "Michiel" },
                    Score = 29,
                    Duration = TimeSpan.FromSeconds(134)
                });
                model.GameEntries.Add(new GameEntry
                {
                    User = new User { Name = "Samwise" },
                    Score = 23,
                    Duration = TimeSpan.FromSeconds(100)
                });

                return View(model);
            }
        }
    }
}