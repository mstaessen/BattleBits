using System.ComponentModel.DataAnnotations;

namespace BattleBits.Web.Models
{
    public class Competition
    {
        [Key]
        public int Id { get; set; }

        public GameType GameType { get; set; }

        public string Name { get; set; }

    }
}