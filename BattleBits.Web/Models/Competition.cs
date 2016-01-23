using System.Collections.Generic;

namespace BattleBits.Web.Hubs
{
    public class Competition
    {
        public virtual ISet<Game> Games { get; set; }

        public virtual Game ActiveGame { get; set; }
    }
}