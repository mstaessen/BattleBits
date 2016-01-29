using Newtonsoft.Json;

namespace BattleBits.Web.Events
{
    public class BattleBitsPlayerScoredEvent
    {
        [JsonProperty("userId")]
        public string UserId { get; set; }

        [JsonProperty("time")]
        public double Time { get; set; }

        [JsonProperty("score")]
        public double Score { get; set; }
    }
}