using Newtonsoft.Json;

namespace BattleBits.Web.DTO
{
    public class BattleBitsScoreDTO
    {
        [JsonProperty("time")]
        public double Time { get; set; }

        [JsonProperty("score")]
        public double Score { get; set; }

        [JsonProperty("player")]
        public BattleBitsPlayerDTO BattleBitsPlayer { get; set; }
    }
}