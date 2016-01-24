using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BattleBits.Web.Events;
using BattleBits.Web.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace BattleBits.Web.Hubs
{
    [HubName("BattleBitsHub")]
    public class BattleBitsHub : Hub<IBattleBitsClient>
    {
        private static readonly IDictionary<int, BattleBitsSession> ActiveSessions = new Dictionary<int, BattleBitsSession>();

        public Task JoinCompetition(int competitionId)
        {
            return Groups.Add(Context.ConnectionId, FormatCompetitionGroupName(competitionId));
        }

        public void JoinGame(int competitionId)
        {
            var game = GetGame(competitionId);
            Clients.Group(FormatCompetitionGroupName(competitionId)).PlayerJoined(new BattleBitsPlayerJoinedEvent {
                UserId = Context.User.Identity.GetUserId(),
                UserName = Context.User.Identity.GetUserName()
            });
        }

        private BattleBitsGame GetGame(int competitionId)
        {
            var session = GetSession(competitionId);
            if (session.Game == null) {
                var date = DateTime.UtcNow;
                session.Game = new BattleBitsGame(session.NumberCount, new Game {
                    StartTime = date.AddSeconds(15),
                    EndTime = date.AddSeconds(60)
                });
                Clients.Group(FormatCompetitionGroupName(competitionId)).GameScheduled(CreateBattleBitsGameDTO(session));
            }
            return session.Game;
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
            var game = GetGame(competitionId);
            if (game.Bytes[number] != value) {
                throw new Exception("Incorrect Answer");
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

        private static string FormatCompetitionGroupName(int competitionId)
        {
            return $"Competition_{competitionId}";
        }
    }

    public class BattleBitsSession
    {
        public BattleBitsGame Game { get; set; }

        public int CompetitionId { get; set; }

        public string CompetitionName { get; set; }

        public int NumberCount { get; set; }

        public int Duration { get; set; }
    }
}