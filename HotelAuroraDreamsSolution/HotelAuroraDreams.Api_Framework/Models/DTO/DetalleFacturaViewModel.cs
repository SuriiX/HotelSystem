namespace HotelAuroraDreams.Api_Framework.Models.DTO
{
    public class DetalleFacturaViewModel
    {
        public int DetalleFacturaID { get; set; }
        public string TipoConcepto { get; set; }
        public int? ReferenciaID { get; set; } // Puede ser habitacion_id, servicio_id, paquete_id
        public string DescripcionConcepto { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }
}