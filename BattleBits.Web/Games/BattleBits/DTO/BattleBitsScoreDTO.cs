using Newtonsoft.Json;

namespace BattleBits.Web.Games.BattleBits.DTO
{
    public class BattleBitsScoreDTO
    {
        [JsonProperty("rank")]
        public int Rank { get; set; }

        [JsonProperty("time")]
        public double Time { get; set; }

        [JsonProperty("score")]
        public double Score { get; set; }

        [JsonProperty("player")]
        public BattleBitsPlayerDTO Player { get; set; }
    }
}