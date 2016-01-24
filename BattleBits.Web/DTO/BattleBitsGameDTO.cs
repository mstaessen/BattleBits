using System;
using Newtonsoft.Json;

namespace BattleBits.Web.DTO
{
    public class BattleBitsGameDTO
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("start")]
        public DateTime StartTime { get; set; }

        [JsonProperty("duration")]
        public int Duration { get; set; }
    }
}