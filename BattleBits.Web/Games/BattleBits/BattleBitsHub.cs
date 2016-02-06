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

            if (gameCreated) {
                Clients.Group(FormatCompetitionGroupName(competitionId)).GameScheduled(CreateGameScheduledEvent(game));
            } else {
                Clients.Group(FormatCompetitionGroupName(competitionId)).PlayerJoined(CreatePlayerJoinedEvent(player));
            }
        }

        private static BattleBitsPlayerLeftEvent CreatePlayerLeftEvent(string playerId)
        {
            return new BattleBitsPlayerLeftEvent {
                UserId = playerId
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

            var score = game.Scores[Context.User.Identity.GetUserId()];
            if (score == null) {
                throw new Exception("Player not found.");
            }
            if(score.Value != number)
            {
                throw new Exception("This number is not permitted at this time");
            }
            if (game.EndTime >= DateTime.UtcNow) // answer only accepted in time
            {
                score.Value++;
                score.Time = DateTime.UtcNow - game.StartTime;

                Clients.Group(FormatCompetitionGroupName(competitionId)).PlayerScored(CreatePlayerScoredEvent(score));
            }
        }


        public override Task OnDisconnected(bool stopCalled)
        {
            // BUG: if a user is connected twice and disconnects in one place, player is "kicked" from game.
            var playerId = Context.User.Identity.GetUserId();
            if (playerId != null) {
                foreach (var session in ActiveSessions.Values) {
                    var game = session.CurrentOrNextGame;
                    if (game == null) continue; // no game in progress
                    if (!game.Scores.ContainsKey(playerId)) continue; // player that's leaving has no score
                    // notify clients player left
                    Clients.Group(FormatCompetitionGroupName(session.CompetitionMeta.Competition.Id)).PlayerLeft(CreatePlayerLeftEvent(playerId));
                    var score = game.Scores[playerId];
                    if (score.Value > 0) continue;
                    game.Scores.Remove(playerId);
                    if (!game.Scores.Any()) {
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
                    .ThenBy(x => x.Time)
                    .Take(BattleBitsSession.HighScoreCount)
                    .ToList();
                return new BattleBitsSession {
                    CompetitionMeta = competition,
                    PreviousGameScores = GetPreviousGameScores(competition, context),
                    HighScores = highScores
                };
            }
        }

        private static IList<BattleBitsScore> GetPreviousGameScores(BattleBitsCompetitionMeta competitionMeta, CompetitionContext context)
        {
            var startTime = context.Scores
                .Where(x => x.Game.Competition.Id == competitionMeta.Id)
                .Select(s => s.Game.StartTime)
                .DefaultIfEmpty()
                .Max();
            return context.Scores
                .Where(x => x.Game.Competition.Id == competitionMeta.Id
                    && x.Game.StartTime == startTime)
                .Join(context.Users, s => s.UserId, u => u.Id, (s, u) =>
                    new BattleBitsScore {
                        Value = s.Value,
                        Time = s.Time,
                        Player = new BattleBitsPlayer {
                            UserId = u.Id,
                            UserName = u.UserName,
                            Company = u.Company,
                        }
                    }).OrderByDescending(s => s.Value)
                .ThenBy(s => s.Time)
                .ToList();
        }

        private bool GetOrCreateGame(BattleBitsSession session, out BattleBitsGame game)
        {
            if (session == null) {
                throw new ArgumentNullException(nameof(session));
            }
            if (session.CurrentOrNextGame != null) {
                game = session.CurrentOrNextGame;
                return false;
            }

            Action<BattleBitsGame> onGameStart = g => {
                var evt = CreateGameStartedEvent(g);
                Clients.Group(FormatCompetitionGroupName(session.CompetitionMeta.Id)).GameStarted(evt);
            };
            Action<BattleBitsGame, IList<BattleBitsScore>> onGameEnd = (g, scores) => {
                var evt = CreateGameEndedEvent(g.Scores.Values, scores);
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

        private static BattleBitsGameEndedEvent CreateGameEndedEvent(IEnumerable<BattleBitsScore> previousGameScores, IEnumerable<BattleBitsScore> allScores)
        {
            return new BattleBitsGameEndedEvent {
                PreviousGameScores = previousGameScores.Select(CreateScoreDTO).ToList(),
                HighScores = allScores.Select(CreateScoreDTO).ToList()
            };
        }

        private static BattleBitsScoreDTO CreateScoreDTO(BattleBitsScore score, int rank)
        {
            return new BattleBitsScoreDTO {
                Rank = rank + 1,
                Player = CreatePlayerDTO(score.Player),
                Score = score.Value,
                Time = score.Time.TotalSeconds
            };
        }

        private static BattleBitsSessionDTO CreateSessionDTO(BattleBitsSession session)
        {
            return new BattleBitsSessionDTO {
                Competition = new BattleBitsCompetitionDTO {
                    Id = session.CompetitionMeta.Id,
                    Name = session.CompetitionMeta.Competition.Name,
                },
                HighScores = session.HighScores.Select(CreateScoreDTO).ToList(),
                PreviousGameScores = session.PreviousGameScores.Select(CreateScoreDTO).ToList(),
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
                Scores = game.Scores.Values
                    .Select(s =>
                        new BattleBitsScoreDTO {
                            Player = CreatePlayerDTO(s.Player, s.Value),
                            Rank = rank++,
                            Score = s.Value,
                            Time = s.Time.TotalSeconds
                        }).ToDictionary(s => s.Player.UserId)
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