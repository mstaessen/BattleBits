using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace BattleBits.Web.Events
{
    public class BattleBitsGameEndedEvent
    {
        [JsonProperty("start")]
        public DateTime StartTime { get; set; }

        [JsonProperty("end")]
        public DateTime EndTime { get; set; }

        [JsonProperty("duration")]
        public int Duration { get; set; }

        [JsonProperty("scores")]
        public IList<ScoreDTO> Scores { get; set; }
    }
}