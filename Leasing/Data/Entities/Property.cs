using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Leasing.Data.Entities
{
    public class Property
    {
        public int Id { get; set; }

        [Display(Name = "Localidad")]
        [MaxLength(50, ErrorMessage = "The {0} field can not have more than {1} characters.")]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string Neighborhood { get; set; }

        [Display(Name = "Dirección")]
        [MaxLength(50, ErrorMessage = "The {0} field can not have more than {1} characters.")]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string Address { get; set; }

        [Display(Name = "Precio")]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = false)]
        public decimal Price { get; set; }

        [Display(Name = "Mts^2")]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public int SquareMeters { get; set; }

        [Display(Name = "Habitaciones")]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public int Rooms { get; set; }

        [Display(Name = "Estrato")]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public int Stratum { get; set; }

        [Display(Name = "Tiene Estacionamiento?")]
        public bool HasParkingLot { get; set; }

        [Display(Name = "Esta Disponible?")]
        public bool IsAvailable { get; set; }

        [Display(Name = "Descripción")]
        public string Remarks { get; set; }

        [Display(Name = "Latitud")]
        [DisplayFormat(DataFormatString = "{0:N6}")]
        public double Latitude { get; set; }

        [Display(Name = "Longitud")]
        [DisplayFormat(DataFormatString = "{0:N6}")]
        public double Longitude { get; set; }

        public PropertyType PropertyType { get; set; }

        public Owner Owner { get; set; }

        public ICollection<PropertyImage> PropertyImages { get; set; }

        public ICollection<Contract> Contracts { get; set; }

        public string FirstImage
        {
            get
            {
                if (PropertyImages == null || PropertyImages.Count == 0)
                {
                    return "https://myleasing.azurewebsites.net/images/Properties/noImage.png";
                }

                return PropertyImages.FirstOrDefault().ImageUrl;
            }
        }
    }
}
