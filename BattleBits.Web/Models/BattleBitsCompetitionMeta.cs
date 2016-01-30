using System;
using System.ComponentModel.DataAnnotations;

namespace BattleBits.Web.Models
{
    public class BattleBitsCompetitionMeta
    {
        [Key]
        public int Id { get; protected set; }

        public byte NumberCount { get; protected set; }

        public TimeSpan Duration { get; protected set; }

        public virtual Competition Competition { get; protected set; }

        /// <summary>
        /// EF constructor
        /// </summary>
        protected BattleBitsCompetitionMeta() {}

        public BattleBitsCompetitionMeta(string name, byte numberCount = 24, int seconds = 45)
        {
            Competition = new Competition {
                GameType = GameType.BattleBits,
                Name = name
            };
            NumberCount = numberCount;
            Duration = TimeSpan.FromSeconds(seconds);
        }
    }
}