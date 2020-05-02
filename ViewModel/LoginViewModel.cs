using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;

namespace FirstSample.ViewModel
{
    public class LoginViewModel
    {
        [Required]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name="Remember Me")]
        public bool RememberMe { get; set; }

        public string ReturnURL { get; set; }

        public List<AuthenticationScheme> ExternalLogin {get;set;}

    }
}