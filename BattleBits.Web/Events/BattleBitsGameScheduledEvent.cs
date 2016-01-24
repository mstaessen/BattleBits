using System;
using Newtonsoft.Json;

namespace BattleBits.Web.Events
{
    public class BattleBitsGameScheduledEvent
    {
        [JsonProperty("start")]
        public DateTime StartTime { get; set; }

        [JsonProperty("duration")]
        public int Duration { get; set; }
    }
}