using System;

namespace HotelAuroraDreams.Api_Framework.Models.DTO
{
    public class CheckInViewModel
    {
        public int CheckInID { get; set; }
        public int ReservaID { get; set; }
        public string NombreCliente { get; set; }
        public DateTime FechaHora { get; set; }
        public string MetodoPagoAdelanto { get; set; }
        public bool DocumentosVerificados { get; set; }
        public string Observaciones { get; set; }
        public string NombreEmpleado { get; set; }
    }
}