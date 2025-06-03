using System.ComponentModel.DataAnnotations;

namespace HotelAuroraDreams.WebApp_Framework.Models
{
    public class CheckOutBindingModel
    {
        [Required(ErrorMessage = "El ID de la Reserva es requerido.")]
        public int ReservaID { get; set; }

        [Required(ErrorMessage = "El método de pago final es requerido.")]
        [StringLength(50)]
        public string MetodoPagoFinal { get; set; }

        public string Observaciones { get; set; }
    }
}