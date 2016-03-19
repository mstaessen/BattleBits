using Newtonsoft.Json;

namespace BattleBits.Web.Games.BattleBits.DTO
{
    public class BattleBitsPlayerDTO
    {
        [JsonProperty("userId")]
        public string UserId { get; set; }

        [JsonProperty("userName")]
        public string UserName { get; set; }

        [JsonProperty("school")]
        public string School { get; set; }

        [JsonProperty("highScore")]
        public double? HighScore { get; set; }
    }
}