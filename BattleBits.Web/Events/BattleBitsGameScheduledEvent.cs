using System;
using System.Collections.Generic;
using BattleBits.Web.DTO;
using Newtonsoft.Json;

namespace BattleBits.Web.Events
{
    public class BattleBitsGameScheduledEvent
    {
        [JsonProperty("start")]
        public DateTime StartTime { get; set; }

        [JsonProperty("delay")]
        public int Delay { get; set; }

        [JsonProperty("duration")]
        public int Duration { get; set; }

        public IList<BattleBitsPlayerDTO> Players { get; set; } = new List<BattleBitsPlayerDTO>();
    }
}