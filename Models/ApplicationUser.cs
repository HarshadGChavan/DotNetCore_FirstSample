using Microsoft.AspNetCore.Identity;

namespace FirstSample.Models
{
    public class ApplicationUser: IdentityUser
    {
        public string City { get; set; }
    }
}