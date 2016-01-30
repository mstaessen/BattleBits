using System.ComponentModel.DataAnnotations;

namespace BattleBits.Web.Models
{
    public class BattleBitsGameMeta
    {
        [Key]
        public long Id { get; protected set; }
        
        [Required]
        public byte[] Bytes { get; set; }

        [Required]
        public virtual Game Game { get; protected set; }
    }
}