using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BattleBits.Web.Models;

namespace BattleBits.Web.Games.BattleBits.Business
{
    public class BattleBitsSession
    {
        public const int ScoreCount = 20;
        public static TimeSpan GameDelay = TimeSpan.FromSeconds(15);

        private Timer gameStartTimer;
        private Timer gameEndTimer;


        public BattleBitsCompetitionMeta CompetitionMeta { get; set; }

        public BattleBitsGame NextGame { get; private set; }

        public BattleBitsGame CurrentGame { get; private set; }

        public BattleBitsGame PreviousGame { get; internal set; }

        public IList<BattleBitsScore> HighScores { get; set; }

        public BattleBitsGame CreateGame(Action<BattleBitsGame> onGameStart, Action<BattleBitsGame> onGameEnd)
        {
            if (CurrentGame != null || NextGame != null) {
                throw new Exception("Please wait for the current game to end.");
            }

            var date = DateTime.UtcNow;
            var game = new BattleBitsGame(CompetitionMeta.NumberCount) {
                StartTime = date + GameDelay,
                EndTime = date + GameDelay + CompetitionMeta.Duration
            };

            gameStartTimer = new Timer(state => {
                CurrentGame = game;
                NextGame = null;
                onGameStart(CurrentGame);
                gameStartTimer.Dispose();
                gameStartTimer = null;
            }, null, game.StartTime - DateTime.UtcNow, Timeout.InfiniteTimeSpan);

            gameEndTimer = new Timer(state => {
                using (var context = new CompetitionContext()) {
                    var competition = context.Competitions.FirstOrDefault(x => x.Id == CompetitionMeta.Id);
                    if (competition != null) {
                        competition.Games.Add(CreateGameModel(game));
                        context.SaveChanges();
                    }
                }
                onGameEnd(game);
                gameEndTimer.Dispose();
                gameEndTimer = null;
                UpdateHighScores(game.Scores);
                PreviousGame = game;
                CurrentGame = null;
            }, null, game.EndTime - DateTime.UtcNow, Timeout.InfiniteTimeSpan);

            NextGame = game;
            return game;
        }

        private Game CreateGameModel(BattleBitsGame game)
        {
            var model = new Game {
                StartTime = game.StartTime,
                EndTime = game.EndTime
            };
            foreach (var score in game.Scores) {
                model.Scores.Add(new Score {
                    Value = score.Value,
                    Time = score.Time,
                    UserId = score.Player.UserId
                });
            }
            return model;
        }

        private void UpdateHighScores(IEnumerable<BattleBitsScore> scores)
        {
            HighScores = HighScores.Concat(scores)
                .OrderByDescending(x => x.Value)
                .ThenByDescending(x => x.Time)
                .Take(ScoreCount)
                .ToList();
        }

        public bool CancelGame()
        {
            if (CurrentGame != null && CurrentGame.StartTime > DateTime.UtcNow) {
                // Game already started
                return false;
            }

            if (gameStartTimer != null) {
                gameStartTimer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
                gameStartTimer.Dispose();
                gameStartTimer = null;
            }

            if (gameEndTimer != null) {
                gameEndTimer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
                gameEndTimer.Dispose();
                gameEndTimer = null;
            }

            return true;
        }
    }
}