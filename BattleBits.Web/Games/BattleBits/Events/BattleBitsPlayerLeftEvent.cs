using BattleBits.Web.Games.BattleBits.DTO;
using Newtonsoft.Json;

namespace BattleBits.Web.Games.BattleBits.Events
{
    public class BattleBitsPlayerLeftEvent
    {
        [JsonProperty("player")]
        public BattleBitsPlayerDTO Player { get; set; }
    }
}