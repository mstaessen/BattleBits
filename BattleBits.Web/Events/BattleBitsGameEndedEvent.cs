using System;
using System.Collections.Generic;
using BattleBits.Web.DTO;
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
        public IList<BattleBitsScoreDTO> Scores { get; set; }
    }
}