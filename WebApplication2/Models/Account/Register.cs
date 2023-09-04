using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Models
{
    public class RegisterModel
    {
        [Required]
        [Display(Name = "Name")]
        public string? Name { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string? Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string? ConfirmPassword { get; set; }
    }
}
