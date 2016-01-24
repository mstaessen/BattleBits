using System;
using System.Collections.Generic;

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
            FillWithRandomBytes();
        }

        /// <summary>
        /// Random bytes with constraints:
        /// - No 2 bytes are the same
        /// - Zeros are not allowed
        /// - No 2 contiguous bytes have the same 4 bits in front or at the end
        /// - No 2 contiguous bytes have the same 4 bits parts just swapped
        /// </summary>
        private void FillWithRandomBytes() {
            HashSet<int> set = new HashSet<int>();
            int prevA = 0,
                prevB = 0,
                a = 0,
                b = 0;
            for (int i = 0; i < Bytes.Length; i++)
            {
                while (prevA == a
                    || prevB == b
                    || (prevB == a && prevA == b)
                    || !set.Add((a << 4) | b))
                {
                    a = Random.Next(15) + 1;
                    b = Random.Next(15) + 1;
                }
                Bytes[i] = (byte)(a << 4 | b);
                prevA = a;
                prevB = b;
            }
        }
    }
}