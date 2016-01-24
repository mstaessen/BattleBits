using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BattleBits.Web.Models
{
    public class Competition
    {
        [Key]
        public int Id { get; protected set; }

        public GameType GameType { get; set; }

        public string Name { get; set; }

        public virtual ISet<Game> Games { get; set; } 

    }
}