using System.ComponentModel.DataAnnotations;

namespace BattleBits.Web.ViewModels
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; }


        [Required]
        [Display(Name = "Company")]
        public string Company { get; set; }


        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
}
