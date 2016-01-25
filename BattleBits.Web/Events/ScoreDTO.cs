using System;
using Newtonsoft.Json;

namespace BattleBits.Web.Events
{
    public class ScoreDTO
    {
        [JsonProperty("time")]
        public double Time { get; set; }

        [JsonProperty("score")]
        public double Score { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("company")]
        public string Company { get; set; }
    }
}