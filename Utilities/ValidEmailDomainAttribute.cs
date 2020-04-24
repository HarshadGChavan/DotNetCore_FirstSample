using System.ComponentModel.DataAnnotations;

namespace FirstSample.Utilities {
    public class ValidEmailDomainAttribute : ValidationAttribute {
        private readonly string _allowedDomain;
        public ValidEmailDomainAttribute (string allowedDomain) {
            this._allowedDomain = allowedDomain;

        }

        public override bool IsValid (object value) 
        {
            string[] strings =  value.ToString().Split('@');
            return strings[1].ToUpper() == _allowedDomain.ToUpper();
        }

    }
}