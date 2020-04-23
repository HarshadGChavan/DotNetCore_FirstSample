using System.Collections.Generic;
using FirstSample.Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace FirstSample.ViewModel
{
    public class EmployeeCreateViewModel
    {
         [Required]
        [MaxLength(50,ErrorMessage="Name can not exceed 50 characters.")]
        public string Name { get; set; }
        [Required]
        [RegularExpression(@"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$",ErrorMessage="Invalid Email format.")]
        [Display(Name="Official Email")]
        public string Email { get; set; }
        [Required]
        public Dept? Department { get; set; }

        public IFormFile Photo { get; set; }
    }
}