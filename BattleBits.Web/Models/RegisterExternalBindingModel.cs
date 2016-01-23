using System.ComponentModel.DataAnnotations;

namespace BattleBits.Web.Models
{
    public class RegisterExternalBindingModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
}