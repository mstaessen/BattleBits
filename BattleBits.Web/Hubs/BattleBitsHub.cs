using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BattleBits.Web.DTO;
using BattleBits.Web.Events;
using BattleBits.Web.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace BattleBits.Web.Hubs
{
    [Authorize]
    [HubName("BattleBitsHub")]
    public class BattleBitsHub : Hub<IBattleBitsClient>
    {
        private static readonly IDictionary<int, BattleBitsSession> ActiveSessions = new Dictionary<int, BattleBitsSession>();

        public async Task<BattleBitsCompetitionDTO> JoinCompetition(int competitionId)
        {
            var session = GetSession(competitionId);
            session.Players[Context.ConnectionId] = false;
            await Groups.Add(Context.ConnectionId, FormatCompetitionGroupName(competitionId));
            return CreateBattleBitsCompetitionDTO(session);
        }

        public void JoinGame(int competitionId)
        {
            var session = GetSession(competitionId);
            BattleBitsGame game;
            if (GetOrCreateGame(session, out game)) {
                Clients.Group(FormatCompetitionGroupName(competitionId)).GameScheduled(CreateBattleBitsGameDTO(session));
            }

            if (game != null) {
                session.Players[Context.ConnectionId] = true;
                Clients.Group(FormatCompetitionGroupName(competitionId)).PlayerJoined(new BattleBitsPlayerJoinedEvent {
                    UserId = Context.User.Identity.GetUserId(),
                    UserName = Context.User.Identity.GetUserName()
                });
            }
        }

        private static BattleBitsGameScheduledEvent CreateBattleBitsGameDTO(BattleBitsSession competition)
        {
            return new BattleBitsGameScheduledEvent {
                Duration = competition.Duration,
                StartTime = competition.Game.Game.StartTime
            };
        }

        public Task LeaveRoom(string roomName)
        {
            return Groups.Remove(Context.ConnectionId, roomName);
        }

        public int NextNumber(int competitionId, int number, int value)
        {
            var session = GetSession(competitionId);
            var game = session.Game;
            if (game == null) {
                throw new Exception("No current game");
            }

            number++;
            var score = game.Scores.FirstOrDefault(x => x.UserId == Context.User.Identity.GetUserId());
            if (score == null) {
                score = new Score {
                    UserId = Context.User.Identity.GetUserId(),
                };
                game.Scores.Add(score);
            }
            score.Value = number;
            score.Duration = DateTime.UtcNow - game.StartTime;

            return number < game.Bytes.Length ? game.Bytes[number] : 0;
        }


        public override Task OnDisconnected(bool stopCalled)
        {
            // TODO: Stop tracking session if last user disconnects
            // TODO: Cancel scheduled game when last player disconnects
            // Can use BBSession.Players<ConnectionId, Playing or not> to determine this
            return base.OnDisconnected(stopCalled);
        }

        private static BattleBitsSession GetSession(int competitionId)
        {
            if (!ActiveSessions.ContainsKey(competitionId)) {
                using (var context = new CompetitionContext()) {
                    var competition = context.BattleBitCompetitions.FirstOrDefault(x => x.Competition.Id == competitionId);
                    if (competition == null) {
                        throw new Exception("Competition not found");
                    }
                    ActiveSessions[competitionId] = new BattleBitsSession {
                        CompetitionId = competition.Id,
                        CompetitionName = competition.Competition.Name,
                        NumberCount = competition.NumberCount,
                        Duration = Convert.ToInt32(competition.Duration.TotalSeconds)
                    };
                }
            }
            return ActiveSessions[competitionId];
        }

        private bool GetOrCreateGame(BattleBitsSession session, out BattleBitsGame game)
        {
            if (session == null) {
                throw new ArgumentNullException(nameof(session));
            }
            if (session.Game != null) {
                game = session.Game;
                return false;
            }

            Action<BattleBitsGame> onGameStart = g => {
                var evt = CreateGameStartedEvent(g);
                Clients.Group(FormatCompetitionGroupName(session.CompetitionId)).GameStarted(evt);
            };
            Action<BattleBitsGame> onGameEnd = g => {
                var evt = CreateGameEndedEvent(g);
                Clients.Group(FormatCompetitionGroupName(session.CompetitionId)).GameEnded(evt);
            };
            game = session.CreateGame(onGameStart, onGameEnd);
            return true;
        }

        private static BattleBitsGameStartedEvent CreateGameStartedEvent(BattleBitsGame game)
        {
            return new BattleBitsGameStartedEvent {
                EndTime = game.EndTime,
                Duration = Convert.ToInt32(game.Duration.TotalSeconds),
                Numbers = game.Bytes.Select(Convert.ToInt32).ToArray()
            };
        }

        private static BattleBitsGameEndedEvent CreateGameEndedEvent(BattleBitsGame game)
        {
            return new BattleBitsGameEndedEvent {
                StartTime = game.StartTime,
                EndTime = game.EndTime,
                Duration = Convert.ToInt32(game.Duration.TotalSeconds),
                Scores = game.Scores.Select(x => new ScoreDTO {
                    Name = "TODO Name", // TODO,
                    Company = "TODO Company", // TODO
                    Score = x.Value,
                    Time = x.Duration.TotalSeconds
                }).ToList()
            };
        }

        private BattleBitsCompetitionDTO CreateBattleBitsCompetitionDTO(BattleBitsSession session)
        {
            return new BattleBitsCompetitionDTO {
                Id = session.CompetitionId,
                Name = session.CompetitionName
            };
        }

        private static string FormatCompetitionGroupName(int competitionId)
        {
            return $"Competition_{competitionId}";
        }
    }

    public class BattleBitsSession
    {
        private Timer gameStartTimer;   
        private Timer gameEndTimer;   

        public BattleBitsCompetition Competition { get; set; }

        public int CompetitionId { get; set; }

        public string CompetitionName { get; set; }

        public int NumberCount { get; set; }

        public int Duration { get; set; }

        public BattleBitsGame Game { get; private set; }

        public IDictionary<string, bool> Players { get; set; }

        // TODO: Prevent games being created when there is a game running. Or make a queue... But in that case you should also track user-game assignments
        public BattleBitsGame CreateGame(Action<BattleBitsGame> onGameStart, Action<BattleBitsGame> onGameEnd)
        {
            var date = DateTime.UtcNow;
            var game = new BattleBitsGame(NumberCount, new Game {
                StartTime = date.AddSeconds(15),
                EndTime = date.AddSeconds(60)
            });

            gameStartTimer = new Timer(state => {
                onGameStart(Game);
                gameStartTimer.Dispose();
                gameStartTimer = null;
            }, null, Game.StartTime - DateTime.UtcNow, Timeout.InfiniteTimeSpan);

            gameStartTimer = new Timer(state => {
                using (var context = new CompetitionContext()) {
                    var competition = context.Competitions.FirstOrDefault(x => x.Id == CompetitionId);
                    if (competition != null) {
                        competition.Games.Add(Game.Game);
                        context.SaveChanges();
                    }
                }
                onGameEnd(Game);
                gameEndTimer.Dispose();
                gameEndTimer = null;
                Game = null;

                foreach (var connectionId in Players.Keys) {
                    // All players must explicitly join before new game is scheduled.
                    Players[connectionId] = false;
                }
            }, null, Game.EndTime - DateTime.UtcNow, Timeout.InfiniteTimeSpan);

            Game = game;
            return Game;
        }

        public bool CancelGame()
        {
            if (Game != null && Game.StartTime > DateTime.UtcNow) {
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