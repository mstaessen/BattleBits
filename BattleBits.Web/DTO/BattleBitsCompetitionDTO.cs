using Newtonsoft.Json;

namespace BattleBits.Web.DTO
{
    public class BattleBitsCompetitionDTO
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

    }
}