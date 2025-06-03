
using System;

namespace HotelAuroraDreams.WebApp_Framework.Models
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