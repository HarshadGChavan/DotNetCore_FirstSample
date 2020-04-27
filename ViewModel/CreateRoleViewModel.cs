using System.ComponentModel.DataAnnotations;

namespace FirstSample.ViewModel
{
    public class CreateRoleViewModel
    {
        [Required]
        public string RoleName { get; set; }
    }
}