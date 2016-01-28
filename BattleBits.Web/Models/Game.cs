using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BattleBits.Web.Models
{
    public class Game
    {
        [Key]
        public int Id { get; protected set; }

        public virtual ISet<Score> Scores { get; protected set; } = new HashSet<Score>();

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public TimeSpan Duration => EndTime - StartTime;

        public virtual Competition Competition { get; protected set; }

    }
}