using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;

namespace BattleBits.Web.Hubs
{
    public class CompetitionHub : Hub<ICompetitionClient>
    {
        private IDictionary<string, CompetitionSession> competitionSessions = new Dictionary<string, CompetitionSession>(); 

        public Task JoinCompetition(string competitionId)
        {
            return Groups.Add(Context.ConnectionId, competitionId);
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

    internal class CompetitionSession {}
}