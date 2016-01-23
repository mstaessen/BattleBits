
using BattleBits.Web.Models;
using System.Collections.Generic;

namespace BattleBits.Web.ViewModels
{
    public class CompetitionRankingViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public IList<GameEntry> GameEntries { get; set; }
    }
}