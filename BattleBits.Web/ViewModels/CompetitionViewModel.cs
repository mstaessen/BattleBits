
using BattleBits.Web.Models;

namespace BattleBits.Web.ViewModels
{
    public class CompetitionViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public GameType GameType { get; set; }

        public int NumberOfGames { get; set; }
    }
}