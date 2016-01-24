using Newtonsoft.Json;

namespace BattleBits.Web.Events
{
    public class BattleBitsPlayerJoinedEvent
    {
        [JsonProperty("userId")]
        public string UserId { get; set; }

        [JsonProperty("userName")]
        public string UserName { get; set; }
    }
}