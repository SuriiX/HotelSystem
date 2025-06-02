namespace HotelAuroraDreams.Api_Framework.Models.DTO
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
}