using BattleBits.Web.Games.BattleBits.DTO;
using Newtonsoft.Json;

namespace BattleBits.Web.Games.BattleBits.Events
{
    public class BattleBitsPlayerScoredEvent
    {
        [JsonProperty("score")]
        public BattleBitsScoreDTO Score { get; set; }
    }
}