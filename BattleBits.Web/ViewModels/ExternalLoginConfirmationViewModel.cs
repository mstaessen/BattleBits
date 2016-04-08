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
        [RegularExpression("04[0-9]{8}", ErrorMessage="Phone number is invalid, please use format 04xxxxxxxx")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
}
