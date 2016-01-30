using System;
using System.ComponentModel.DataAnnotations;

namespace BattleBits.Web.Models
{
    public class Score
    {
        [Key]
        public int Id { get; set; }

        public TimeSpan Time { get; set; }

        public double Value { get; set; }

        public virtual Game Game { get; set; }

        public virtual string UserId { get; set; }
    }
}