using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;

namespace BattleBits.Web.Hubs
{
    public class BattleBitsHub : Hub<IBattleBitsClient>
    {
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
}