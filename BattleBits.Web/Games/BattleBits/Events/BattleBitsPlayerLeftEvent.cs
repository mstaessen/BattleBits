using Newtonsoft.Json;

namespace BattleBits.Web.Games.BattleBits.Events
{
    public class BattleBitsPlayerLeftEvent
    {
        [JsonProperty("userId")]
        public string UserId { get; set; }
    }
}