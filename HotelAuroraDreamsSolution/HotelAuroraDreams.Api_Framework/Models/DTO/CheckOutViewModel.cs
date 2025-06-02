using System;

namespace HotelAuroraDreams.Api_Framework.Models.DTO
{
    public class CheckOutViewModel
    {
        public int CheckOutID { get; set; }
        public int ReservaID { get; set; }
        public string NombreCliente { get; set; }
        public DateTime FechaHora { get; set; }
        public decimal TotalFactura { get; set; }
        public string MetodoPagoFinal { get; set; }
        public string Observaciones { get; set; }
        public string NombreEmpleado { get; set; }
        public int? FacturaID { get; set; } // ID de la factura de alojamiento generada
    }
}