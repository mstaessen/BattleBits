using BattleBits.Web.Games.BattleBits.DTO;
using Newtonsoft.Json;

namespace BattleBits.Web.Games.BattleBits.Events
{
    public class BattleBitsGameScheduledEvent
    {
        [JsonProperty("delay")]
        public int Delay { get; set; }

        [JsonProperty("game")]
        public BattleBitsGameDTO Game { get; set; }
    }
}