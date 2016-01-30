using BattleBits.Web.Games.BattleBits.DTO;
using Newtonsoft.Json;

namespace BattleBits.Web.Games.BattleBits.Events
{
    public class BattleBitsGameEndedEvent
    {
        [JsonProperty("game")]
        public BattleBitsGameDTO Game { get; set; }
    }
}