using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FirstSample.ViewModel
{
    public class EditUserViewModel
    {
        public EditUserViewModel()
        {
            Roles=new List<string>();
            Claims = new List<string>();
        }   

        public string Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        public string City { get; set; }

        public List<string> Roles {get;set;}

         public List<string> Claims {get;set;}

    }
}