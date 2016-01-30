using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace BattleBits.Web.Games.BattleBits.DTO
{
    public class BattleBitsGameDTO
    {
        [JsonProperty("start")]
        public DateTime StartTime { get; set; }

        [JsonProperty("end")]
        public DateTime EndTime { get; set; }

        [JsonProperty("duration")]
        public int Duration { get; set; }

        [JsonProperty("numbers")]
        public IList<int> Numbers { get; set; }

        public IList<BattleBitsScoreDTO> Scores { get; set; }
    }
}