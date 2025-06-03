using System;
using System.ComponentModel.DataAnnotations;

namespace HotelAuroraDreams.WebApp_Framework.Models
{
    public class CheckInBindingModel
    {
        [Required(ErrorMessage = "El ID de la Reserva es requerido.")]
        public int ReservaID { get; set; }

        [StringLength(50)]
        public string MetodoPagoAdelanto { get; set; }

        public bool DocumentosVerificados { get; set; } = false;

        public string Observaciones { get; set; }
    }
}