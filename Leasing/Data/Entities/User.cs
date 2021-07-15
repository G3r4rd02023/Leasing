using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Leasing.Data.Entities
{
    public class User : IdentityUser
    {
        [Display(Name = "DNI")]
        [MaxLength(20, ErrorMessage = "The {0} field can not have more than {1} characters.")]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string Document { get; set; }

        [Display(Name = "Nombre")]
        [MaxLength(50, ErrorMessage = "The {0} field can not have more than {1} characters.")]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string FirstName { get; set; }

        [Display(Name = "Apellidos")]
        [MaxLength(50, ErrorMessage = "The {0} field can not have more than {1} characters.")]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string LastName { get; set; }

        [Display(Name = "Dirección")]
        [MaxLength(100, ErrorMessage = "The {0} field can not have more than {1} characters.")]
        public string Address { get; set; }

        [Display(Name = "Propietario")]
        public string FullName => $"{FirstName} {LastName}";

        [Display(Name = "Propietario")]
        public string FullNameWithDocument => $"{FirstName} {LastName} - {Document}";
    }
}
