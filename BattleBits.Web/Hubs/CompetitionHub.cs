using System.Collections.Generic;
using Microsoft.AspNet.SignalR;

namespace BattleBits.Web.Hubs
{
    public class CompetitionHub : Hub<ICompetitionClient>
    {
        private ISet<Competition> activeCompetitions = new HashSet<Competition>();



    }

    public class Competition
    {
        public virtual ISet<Game> Games { get; set; }

        public virtual Game ActiveGame { get; set; }
    }

    public class Game {}

    public interface ICompetitionClient
    {
        void Guess();
    }
}