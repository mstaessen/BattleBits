using System;
using System.ComponentModel.DataAnnotations;

namespace BattleBits.Web.Models
{
    public class GameEntry
    {
        [Key]
        public int Id { get; set; }

        public DateTime StartDate { get; set; }
        public TimeSpan Duration { get; set; }
        public double Score { get; set; }

        public virtual Competition Competition { get; set; }
        public virtual User User { get; set; }

        /// <summary>
        /// EF constructor
        /// </summary>
        public GameEntry() {}
    }
}