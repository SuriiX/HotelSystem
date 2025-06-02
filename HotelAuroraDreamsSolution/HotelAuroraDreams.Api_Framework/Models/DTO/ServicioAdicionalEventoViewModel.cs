namespace HotelAuroraDreams.Api_Framework.Models.DTO
{
	public class ServicioAdicionalEventoViewModel
	{
		public int ServicioAdicionalID { get; set; }
		public string NombreServicio { get; set; }
		public string Descripcion { get; set; }
		public decimal PrecioBase { get; set; }
		public bool RequierePersonalPago { get; set; }
	}
}