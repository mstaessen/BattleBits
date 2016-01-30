using System;

namespace BattleBits.Web.Games.BattleBits.Business
{
    public class BattleBitsScore
    {
        public TimeSpan Time { get; set; }

        public double Value { get; set; }

        public BattleBitsPlayer Player { get; set; }
    }
}