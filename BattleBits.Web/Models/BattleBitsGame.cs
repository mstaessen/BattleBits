using System;

namespace BattleBits.Web.Models
{
    public class BattleBitsGame
    {
        private static readonly Random Random = new Random();

        public long Id { get; set; } 

        public DateTime StartTime { get; set; }

        public TimeSpan Duration { get; set; }

        public byte[] Bytes { get; set; }

        public virtual Competition Competition { get; set; }

        /// <summary>
        /// EF constructor
        /// </summary>
        protected BattleBitsGame() {}

        public BattleBitsGame(int size)
        {
            Bytes = new byte[size];
            Random.NextBytes(Bytes);
        }
    }
}