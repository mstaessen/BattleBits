using System;
using Newtonsoft.Json;

namespace BattleBits.Web.Events
{
    public class BattleBitsGameStartedEvent
    {
        [JsonProperty("end")]
        public DateTime EndTime { get; set; }

        [JsonProperty("duration")]
        public int Duration { get; set; }

        [JsonProperty("numbers")]
        public int[] Numbers { get; set; }
    }
}