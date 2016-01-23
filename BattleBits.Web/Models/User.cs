using System.ComponentModel.DataAnnotations;

namespace BattleBits.Web.Models
{
    // TODO michiel: remove this and couple this instead to ApplicationUser
    public class User
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        /// <summary>
        /// EF constructor
        /// </summary>
        public User() {}
    }
}