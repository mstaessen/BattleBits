using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using BattleBits.Web.Events;
using BattleBits.Web.Hubs;

namespace BattleBits.Web.Models
{
    public class BattleBitsGame
    {
        private static readonly Random Random = new Random();

        public long Id { get; protected set; }
        
        public byte[] Bytes { get; set; }

        public virtual Game Game { get; protected set; }

        [NotMapped]
        public ISet<BattleBitsPlayer> Players { get; } = new HashSet<BattleBitsPlayer>();

        [NotMapped]
        public DateTime StartTime {
            get { return Game.StartTime; }
            set { Game.StartTime = value; }
        }

        [NotMapped]
        public DateTime EndTime {
            get { return Game.EndTime; }
            set { Game.EndTime = value; }
        }

        [NotMapped]
        public TimeSpan Duration => Game.Duration;


        [NotMapped]
        public ISet<Score> Scores => Game.Scores;




        /// <summary>
        /// EF constructor
        /// </summary>
        protected BattleBitsGame() {}

        public BattleBitsGame(int size, Game game)
        {
            Bytes = new byte[size];
            FillWithRandomBytes();
            Game = game;
        }

        /// <summary>
        /// Random bytes with constraints:
        /// - No 2 bytes are the same
        /// - Zeros are not allowed
        /// - No 2 contiguous bytes have the same 4 bits in front or at the end
        /// - No 2 contiguous bytes have the same 4 bits parts just swapped
        /// </summary>
        private void FillWithRandomBytes()
        {
            var set = new HashSet<int>();
            int prevA = 0,
                prevB = 0,
                a = 0,
                b = 0;
            for (var i = 0; i < Bytes.Length; i++) {
                while (prevA == a
                    || prevB == b
                    || (prevB == a && prevA == b)
                    || !set.Add((a << 4) | b)) {
                    a = Random.Next(15) + 1;
                    b = Random.Next(15) + 1;
                }
                Bytes[i] = (byte) (a << 4 | b);
                prevA = a;
                prevB = b;
            }
        }
    }
}