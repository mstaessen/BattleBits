using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using BattleBits.Web.Games.BattleBits.Business;
using BattleBits.Web.Games.BattleBits.DTO;
using BattleBits.Web.Games.BattleBits.Events;
using BattleBits.Web.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace BattleBits.Web.Games.BattleBits
{
    [Authorize]
    [HubName("BattleBitsHub")]
    public class BattleBitsHub : Hub<IBattleBitsClient>
    {
        private static readonly IDictionary<int, BattleBitsSession> ActiveSessions = new Dictionary<int, BattleBitsSession>();

        public async Task<BattleBitsSessionDTO> JoinCompetition(int competitionId)
        {
            var session = GetSession(competitionId);
            await Groups.Add(Context.ConnectionId, FormatCompetitionGroupName(competitionId));
            return CreateSessionDTO(session);
        }

        public void JoinGame(int competitionId)
        {
            var session = GetSession(competitionId);
            BattleBitsGame game;
            var gameCreated = GetOrCreateGame(session, out game);
            var player = GetPlayer(Context.User.Identity.GetUserId());
            game.AddPlayer(player);

            if (gameCreated)
            {
                Clients.Group(FormatCompetitionGroupName(competitionId)).GameScheduled(CreateGameScheduledEvent(game));
            }
            else
            {
                Clients.Group(FormatCompetitionGroupName(competitionId)).PlayerJoined(CreatePlayerJoinedEvent(player));
            }
        }

        private static BattleBitsPlayerLeftEvent CreatePlayerLeftEvent(BattleBitsPlayer player)
        {
            return new BattleBitsPlayerLeftEvent
            {
                Player = new BattleBitsPlayerDTO
                {
                    UserId = player.UserId,
                    UserName = player.UserName,
                    Company = player.Company,
                    HighScore = player.HighScore
                }
            };
        }

        private static BattleBitsPlayerJoinedEvent CreatePlayerJoinedEvent(BattleBitsPlayer player)
        {
            return new BattleBitsPlayerJoinedEvent {
                Player = new BattleBitsPlayerDTO {
                    UserId = player.UserId,
                    UserName = player.UserName,
                    Company = player.Company,
                    HighScore = player.HighScore
                }
            };
        }

        private static BattleBitsPlayer GetPlayer(string userId)
        {
            using (var context = new CompetitionContext()) {
                var user = context.Users.FirstOrDefault(x => x.Id == userId);
                if (user == null) {
                    throw new Exception("User not found");
                }
                var highScore = context.Scores.Where(x => x.UserId == userId).OrderByDescending(x => x.Value).FirstOrDefault();
                return new BattleBitsPlayer {
                    UserId = user.Id,
                    UserName = user.UserName,
                    Company = user.Company,
                    HighScore = highScore?.Value
                };
            }
        }

        private static BattleBitsGameScheduledEvent CreateGameScheduledEvent(BattleBitsGame game)
        {
            return new BattleBitsGameScheduledEvent {
                Delay = Convert.ToInt32((game.StartTime - DateTime.UtcNow).TotalSeconds),
                Game = CreateGameDTO(game)
            };
        }

        public Task LeaveRoom(string roomName)
        {
            return Groups.Remove(Context.ConnectionId, roomName);
        }

        public void NextNumber(int competitionId, int number, int value)
        {
            var session = GetSession(competitionId);
            var game = session.CurrentGame;
            if (game == null) {
                throw new Exception("No current game");
            }

            if (game.Bytes[number] != value) {
                throw new Exception("Incorrect number");
            }

            var score = game.Scores.FirstOrDefault(x => x.Player.UserId == Context.User.Identity.GetUserId());
            if (score == null) {
                throw new Exception("Player not found.");
            }
            score.Value++;
            score.Time = DateTime.UtcNow - game.StartTime;

            Clients.Group(FormatCompetitionGroupName(competitionId)).PlayerScored(CreatePlayerScoredEvent(score));
        }


        public override Task OnDisconnected(bool stopCalled)
        {
            var player = GetPlayer(Context.User.Identity.GetUserId());
            if (player != null)
            {
                foreach (var session in ActiveSessions.Values)
                {
                    var game = session.CurrentOrNextGame;
                    if (game == null) continue;
                    // notify clients player left
                    Clients.Group(FormatCompetitionGroupName(session.CompetitionMeta.Competition.Id)).PlayerLeft(CreatePlayerLeftEvent(player));
                    var scores = game.Scores.Where(s => s.Player.UserId == player.UserId && s.Value == 0).ToList();
                    if (!scores.Any()) continue;
                    foreach (var score in scores)
                    {
                        game.Scores.Remove(score);
                    }
                    if (!game.Scores.Any())
                    {
                        // Cancel scheduled game when last player disconnects
                        session.CancelGame();
                        // TODO: Push cancelling to all clients or handle clientside on playerleft?
                    }
                }
            }
            return base.OnDisconnected(stopCalled);
        }

        private static BattleBitsSession GetSession(int competitionId)
        {
            if (!ActiveSessions.ContainsKey(competitionId)) {
                ActiveSessions[competitionId] = CreateBattleBitsSession(competitionId);
            }
            return ActiveSessions[competitionId];
        }

        private static BattleBitsSession CreateBattleBitsSession(int competitionId)
        {
            using (var context = new CompetitionContext()) {
                var competition = context.BattleBitCompetitions
                    .Include(x => x.Competition)
                    .FirstOrDefault(x => x.Competition.Id == competitionId);
                if (competition == null) {
                    throw new Exception("Competition not found");
                }


                var highScores = context.Scores
                    .Where(x => x.Game.Competition.Id == competitionId)
                    .Join(context.Users, s => s.UserId, u => u.Id, (s, u) => new BattleBitsScore {
                        Player = new BattleBitsPlayer {
                            UserId = u.Id,
                            UserName = u.UserName,
                            Company = u.Company,
                            HighScore = s.Value
                        },
                        Value = s.Value,
                        Time = s.Time
                    })
                    .OrderByDescending(x => x.Value)
                    .ThenByDescending(x => x.Time)
                    .Take(BattleBitsSession.ScoreCount)
                    .ToList();
                return new BattleBitsSession {
                    CompetitionMeta = competition,
                    PreviousGame = GetPreviousGame(competition, context),
                    HighScores = highScores
                };
            }
        }

        private static BattleBitsGame GetPreviousGame(BattleBitsCompetitionMeta competitionMeta, CompetitionContext context)
        {
            var scores = context.Scores
                .Where(x => x.Game.Competition.Id == competitionMeta.Id)
                .Join(context.Users, s => s.UserId, u => u.Id, (s, u) => new {
                    GameStart = s.Game.StartTime,
                    Score = new BattleBitsScore {
                        Value = s.Value,
                        Time = s.Time,
                        Player = new BattleBitsPlayer {
                            UserId = u.Id,
                            UserName = u.UserName,
                            Company = u.Company,
                        }
                    }
                })
                .OrderByDescending(x => x.GameStart)
                .GroupBy(x => x.GameStart)
                .FirstOrDefault();
            var game = new BattleBitsGame(competitionMeta.NumberCount);
            if (scores != null) {
                foreach (var score in scores.Select(x => x.Score)) {
                    game.Scores.Add(score);
                }
            }
            return game;
        }

        private bool GetOrCreateGame(BattleBitsSession session, out BattleBitsGame game)
        {
            if (session == null) {
                throw new ArgumentNullException(nameof(session));
            }
            if(session.CurrentOrNextGame != null)
            {
                game = session.CurrentOrNextGame;
                return false;
            }

            Action<BattleBitsGame> onGameStart = g => {
                var evt = CreateGameStartedEvent(g);
                Clients.Group(FormatCompetitionGroupName(session.CompetitionMeta.Id)).GameStarted(evt);
            };
            Action<BattleBitsGame, IList<BattleBitsScore>> onGameEnd = (g, scores) => {
                var evt = CreateGameEndedEvent(g, scores);
                Clients.Group(FormatCompetitionGroupName(session.CompetitionMeta.Id)).GameEnded(evt);
            };
            game = session.CreateGame(onGameStart, onGameEnd);
            return true;
        }

        private static BattleBitsGameStartedEvent CreateGameStartedEvent(BattleBitsGame game)
        {
            return new BattleBitsGameStartedEvent {
                Game = CreateGameDTO(game)
            };
        }

        private static BattleBitsGameEndedEvent CreateGameEndedEvent(BattleBitsGame game, IList<BattleBitsScore> scores)
        {
            return new BattleBitsGameEndedEvent {
                Game = CreateGameDTO(game),
                HighScores = scores.Select(CreateScoreDTO).ToList()
            };
        }

        private static BattleBitsScoreDTO CreateScoreDTO(BattleBitsScore score, int rank)
        {
            return new BattleBitsScoreDTO {
                Rank = rank,
                Player = CreatePlayerDTO(score.Player),
                Score = score.Value,
                Time = score.Time.TotalSeconds
            };
        }

        private static BattleBitsSessionDTO CreateSessionDTO(BattleBitsSession session)
        {
            var rank = 1;
            return new BattleBitsSessionDTO {
                Competition = new BattleBitsCompetitionDTO {
                    Id = session.CompetitionMeta.Id,
                    Name = session.CompetitionMeta.Competition.Name,
                },
                HighScores = session.HighScores.Select(x => new BattleBitsScoreDTO {
                    Rank = rank++,
                    Score = x.Value,
                    Time = x.Time.TotalSeconds,
                    Player = new BattleBitsPlayerDTO {
                        UserId = x.Player.UserId,
                        UserName = x.Player.UserName,
                        Company = x.Player.Company,
                        HighScore = x.Player.HighScore
                    }
                }).ToList(),
                PreviousGame = CreateGameDTO(session.PreviousGame),
                CurrentGame = CreateGameDTO(session.CurrentGame),
                NextGame = CreateGameDTO(session.NextGame)
            };
        }

        private static BattleBitsGameDTO CreateGameDTO(BattleBitsGame game)
        {
            if (game == null) {
                return null;
            }

            var rank = 1;
            return new BattleBitsGameDTO {
                StartTime = game.StartTime,
                EndTime = game.EndTime,
                Duration = Convert.ToInt32(game.Duration.TotalSeconds),
                Numbers = game.Bytes.Select(Convert.ToInt32).ToList(),
                Scores = game.Scores
                    .OrderByDescending(x => x.Value)
                    .ThenByDescending(x => x.Time)
                    .Select(s => new BattleBitsScoreDTO {
                        Player = CreatePlayerDTO(s.Player, s.Value),
                        Rank = rank++,
                        Score = s.Value,
                        Time = s.Time.TotalSeconds
                    }).ToList()
            };
        }

        private static BattleBitsPlayerDTO CreatePlayerDTO(BattleBitsPlayer player, double? newHighScore = null)
        {
            if (player == null) {
                return null;
            }

            return new BattleBitsPlayerDTO {
                UserId = player.UserId,
                UserName = player.UserName,
                Company = player.Company,
                HighScore = player.HighScore.HasValue 
                    ? Math.Max(newHighScore ?? 0, player.HighScore.Value) 
                    : newHighScore ?? 0
            };
        }

        private static BattleBitsPlayerScoredEvent CreatePlayerScoredEvent(BattleBitsScore score)
        {
            return new BattleBitsPlayerScoredEvent {
                Score = new BattleBitsScoreDTO {
                    Player = CreatePlayerDTO(score.Player),
                    Time = score.Time.TotalSeconds,
                    Score = score.Value
                }
            };
        }

        private static string FormatCompetitionGroupName(int competitionId)
        {
            return $"Competition_{competitionId}";
        }
    }
}