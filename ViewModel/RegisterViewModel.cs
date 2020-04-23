using System.ComponentModel.DataAnnotations;

namespace FirstSample.ViewModel
{
    public class RegisterViewModel
    {
        
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [Display(Name= "Confirm Password")]
        [DataType(DataType.Password)]
        [Compare("Password",ErrorMessage="Password and confirm password do not match.")]
        public string ConfirmPassword { get; set; }

    }
}