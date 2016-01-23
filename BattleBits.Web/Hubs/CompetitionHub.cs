using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;

namespace BattleBits.Web.Hubs
{
    public class CompetitionHub : Hub<ICompetitionClient>
    {
        private ISet<Competition> activeCompetitions = new HashSet<Competition>();

        public Task JoinCompetition(string roomName, string userId)
        {
            return Groups.Add(Context.ConnectionId, roomName);
        }

        public Task LeaveRoom(string roomName)
        {
            return Groups.Remove(Context.ConnectionId, roomName);
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            return base.OnDisconnected(stopCalled);
        }
    }
}