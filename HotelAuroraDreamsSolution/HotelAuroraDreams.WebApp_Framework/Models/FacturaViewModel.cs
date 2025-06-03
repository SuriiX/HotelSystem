using System;
using System.Collections.Generic;

namespace HotelAuroraDreams.WebApp_Framework.Models
{
    public class DetalleFacturaViewModel
    {
        public int DetalleFacturaID { get; set; }
        public string TipoConcepto { get; set; }
        public int? ReferenciaID { get; set; }
        public string DescripcionConcepto { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }

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