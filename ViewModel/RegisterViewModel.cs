using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using  FirstSample.Utilities;

namespace FirstSample.ViewModel
{
    public class RegisterViewModel
    {
        
        [Required]
        [EmailAddress]
        [Remote(action:"IsEmailAvailable",controller:"Account")]
        [ValidEmailDomain(allowedDomain:"Gmail.com",
                          ErrorMessage="Email domain must be Gmail.com")]
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