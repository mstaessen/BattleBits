using Newtonsoft.Json;

namespace BattleBits.Web.Events
{
    public class BattleBitsPlayerLeftEvent
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}