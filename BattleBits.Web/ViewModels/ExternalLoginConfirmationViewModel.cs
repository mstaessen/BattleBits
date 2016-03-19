using System.ComponentModel.DataAnnotations;

namespace BattleBits.Web.ViewModels
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "School")]
        public string School { get; set; }

        [Required]
        [Display(Name = "Study")]
        public string Study { get; set; }

        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
}
