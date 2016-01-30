using System.Collections.Generic;
using Newtonsoft.Json;

namespace BattleBits.Web.Games.BattleBits.DTO
{
    public class BattleBitsSessionDTO
    {
        [JsonProperty("competition")]
        public BattleBitsCompetitionDTO Competition { get; set; }

        [JsonProperty("highScores")]
        public IList<BattleBitsScoreDTO> HighScores { get; set; }

        [JsonProperty("nextGame")]
        public BattleBitsGameDTO NextGame { get; set; }

        [JsonProperty("currentGame")]
        public BattleBitsGameDTO CurrentGame { get; set; }

        [JsonProperty("previousGame")]
        public BattleBitsGameDTO PreviousGame { get; set; }
    }
}