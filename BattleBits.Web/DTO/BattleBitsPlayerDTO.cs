using Newtonsoft.Json;

namespace BattleBits.Web.DTO
{
    public class BattleBitsPlayerDTO
    {
        [JsonProperty("userId")]
        public string UserId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("company")]
        public string Company { get; set; }

        [JsonProperty("highScore")]
        public double? HighScore { get; set; }
    }
}