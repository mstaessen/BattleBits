using System.Collections.Generic;
using BattleBits.Web.Games.BattleBits.DTO;
using Newtonsoft.Json;

namespace BattleBits.Web.Games.BattleBits.Events
{
    public class BattleBitsGameEndedEvent
    {
        [JsonProperty("previousGameScores")]
        public IList<BattleBitsScoreDTO> PreviousGameScores { get; set; }

        [JsonProperty("highScores")]
        public IList<BattleBitsScoreDTO> HighScores { get; set; }
    }
}