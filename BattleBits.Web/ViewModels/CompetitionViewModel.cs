using System;

namespace BattleBits.Web.ViewModels
{
    public class CompetitionViewModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public GameType GameType { get; set; }

        public string Url { get; set; }
    }

    public enum GameType
    {
        Bits
    }
}