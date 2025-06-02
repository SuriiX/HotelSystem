using System;
using System.ComponentModel.DataAnnotations;

namespace HotelAuroraDreams.Api_Framework.Models.DTO
{
    public class CheckInBindingModel
    {
        [Required]
        public int ReservaID { get; set; }
        public string MetodoPagoAdelanto { get; set; } // 'efectivo', 'tarjeta', etc.
        public bool DocumentosVerificados { get; set; } = false;
        public string Observaciones { get; set; }
    }
}