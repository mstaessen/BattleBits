using System;
using System.Collections.Generic;

namespace BattleBits.Web.Games.BattleBits.Business
{
    public class BattleBitsGame
    {
        private static readonly Random Random = new Random();

        public long Id { get; protected set; }
        
        public byte[] Bytes { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public TimeSpan Duration => EndTime - StartTime;

        public IDictionary<string, BattleBitsScore> Scores { get; set; } = new Dictionary<string, BattleBitsScore>();

        public BattleBitsGame(byte size)
        {
            Bytes = GenerateRandomByteArray(size);
        }

        public void AddPlayer(BattleBitsPlayer player)
        {
            if (player == null || Scores.ContainsKey(player.UserId)) {
                return;
            }
            Scores.Add(player.UserId, new BattleBitsScore {
                Player = player,
                Value = 0,
                Time = TimeSpan.Zero
            });
        }

        /// <summary>
        /// Random bytes with constraints:
        /// - No 2 bytes are the same
        /// - Zeros are not allowed
        /// - No 2 contiguous bytes have the same 4 bits in front or at the end
        /// - No 2 contiguous bytes have the same 4 bits parts just swapped
        /// </summary>
        private static byte[] GenerateRandomByteArray(byte length)
        {
            var set = new HashSet<int>();
            var result = new byte[length];
            int prevA = 0,
                prevB = 0,
                a = 0,
                b = 0;
            for (var i = 0; i < result.Length; i++) {
                while (prevA == a
                    || prevB == b
                    || (prevB == a && prevA == b)
                    || !set.Add((a << 4) | b)) {
                    a = Random.Next(15) + 1;
                    b = Random.Next(15) + 1;
                }
                result[i] = (byte) (a << 4 | b);
                prevA = a;
                prevB = b;
            }
            return result;
        }
    }
}