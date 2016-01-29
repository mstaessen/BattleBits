using Newtonsoft.Json;

namespace BattleBits.Web.DTO
{
    public class BattleBitsPlayerDTO
    {
        [JsonProperty("userId")]
        public string UserId { get; set; }

        [JsonProperty("userName")]
        public string UserName { get; set; }

        [JsonProperty("company")]
        public string Company { get; set; }

        [JsonProperty("highScore")]
        public double? HighScore { get; set; }
    }
}