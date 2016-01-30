using Newtonsoft.Json;

namespace BattleBits.Web.Games.BattleBits.DTO
{
    public class BattleBitsCompetitionDTO
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}