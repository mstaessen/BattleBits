using Newtonsoft.Json;

namespace BattleBits.Web.DTO
{
    public class ScoreDTO
    {
        [JsonProperty("time")]
        public double Time { get; set; }

        [JsonProperty("score")]
        public double Score { get; set; }

        [JsonProperty("player")]
        public PlayerDTO Player { get; set; }
    }

    public class PlayerDTO
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("company")]
        public string Company { get; set; }
    }
}