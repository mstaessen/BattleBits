using System;

namespace BattleBits.Web.Models
{
    public class Game
    {
        private static readonly Random Random = new Random();

        public long Id { get; set; } 

        public DateTime StartDate { get; set; }

        public TimeSpan Duration { get; set; }

        public byte[] Bytes { get; set; }

        public virtual Competition Competition { get; set; }

        /// <summary>
        /// EF constructor
        /// </summary>
        protected Game() {}

        public Game(int size)
        {
            Bytes = new byte[size];
            Random.NextBytes(Bytes);
        }
    }
}