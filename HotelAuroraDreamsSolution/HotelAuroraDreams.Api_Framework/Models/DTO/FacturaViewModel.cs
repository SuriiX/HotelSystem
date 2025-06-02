using System;
using System.Collections.Generic;

namespace HotelAuroraDreams.Api_Framework.Models.DTO
{
    public class FacturaViewModel
    {
        public int FacturaID { get; set; }
        public int ReservaID { get; set; }
        public int ClienteID { get; set; }
        public string NombreCliente { get; set; }
        public DateTime FechaEmision { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Impuestos { get; set; }
        public decimal Total { get; set; }
        public string MetodoPagoFactura { get; set; }
        public string Estado { get; set; }
        public string NombreEmpleadoEmisor { get; set; }
        public List<DetalleFacturaViewModel> Detalles { get; set; }
    }
}