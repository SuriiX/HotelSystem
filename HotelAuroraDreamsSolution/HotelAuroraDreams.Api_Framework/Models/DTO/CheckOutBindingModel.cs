using System.ComponentModel.DataAnnotations;

namespace HotelAuroraDreams.Api_Framework.Models.DTO
{
    public class CheckOutBindingModel
    {
        [Required]
        public int ReservaID { get; set; } 
        [Required]
        public string MetodoPagoFinal { get; set; }
        public string Observaciones { get; set; }
    }
}