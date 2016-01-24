using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BattleBits.Web.DTO;
using BattleBits.Web.Models;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace BattleBits.Web.Hubs
{
    [HubName("BattleBitsHub")]
    public class BattleBitsHub : Hub<IBattleBitsClient>
    {
        private static readonly IDictionary<int, BattleBitsCompetition> activeCompetitions = new Dictionary<int, BattleBitsCompetition>(); 

        public Task JoinCompetition(int competitionId)
        {
            if (!activeCompetitions.ContainsKey(competitionId)) {
                activeCompetitions[competitionId] = new BattleBitsCompetition {
                    
                };
            }
            return Groups.Add(Context.ConnectionId, FormatCompetitionGroupName(competitionId));
        }

        private static string FormatCompetitionGroupName(int competitionId)
        {
            return $"Competition_{competitionId}";
        }

        public BattleBitsGameDTO JoinGame(string competitionId)
        {
            throw new NotImplementedException();
        }

        public Task LeaveRoom(string roomName)
        {
            return Groups.Remove(Context.ConnectionId, roomName);
        }

        public int Guess(int currentNumber, int value)
        {
            return new Random().Next(byte.MaxValue);
        }




        public override Task OnDisconnected(bool stopCalled)
        {
            
            return base.OnDisconnected(stopCalled);

        }
    }

    public class BattleBitsCompetition
    {
        public BattleBitsGame Game { get; set; }
    }
}