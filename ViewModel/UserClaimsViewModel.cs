using System.Collections.Generic;

namespace FirstSample.ViewModel
{
    public class UserClaimsViewModel
    {
        public UserClaimsViewModel()
        {
            Claims = new List<UserClaim>();
        }

        public string userID {get;set;}
        public List<UserClaim> Claims {get;set;}

    }
}