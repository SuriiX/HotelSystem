using System;
using System.Collections.Generic;

namespace HotelAuroraDreams.WebApp_Framework.Models
{
    public class DetalleFacturaEventoViewModel
    {
        public int DetalleFacturaEventoID { get; set; }
        public string TipoConcepto { get; set; }
        public int? ReferenciaConceptoID { get; set; }
        public string DescripcionConcepto { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }

    public class FacturaEventoViewModel
    {
        public int FacturaEventoID { get; set; }
        public int ReservaEventoID { get; set; }
        public string NombreEvento { get; set; } // Para mostrar en la lista
        public int ClienteID { get; set; }
        public string NombreCliente { get; set; }
        public DateTime FechaEmision { get; set; }
        public decimal SubtotalSalon { get; set; }
        public decimal SubtotalServiciosAdicionales { get; set; }
        public decimal Impuestos { get; set; }
        public decimal TotalFactura { get; set; }
        public string MetodoPago { get; set; }
        public string Estado { get; set; }
        public string NombreEmpleadoEmisor { get; set; }
        public string Notas { get; set; }
        public List<DetalleFacturaEventoViewModel> Detalles { get; set; }
    }
}