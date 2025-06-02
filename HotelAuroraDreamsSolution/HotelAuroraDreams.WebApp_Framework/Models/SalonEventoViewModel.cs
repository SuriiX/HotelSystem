
namespace HotelAuroraDreams.WebApp_Framework.Models
{
    public class SalonEventoViewModel
    {
        public int SalonEventoID { get; set; }
        public int HotelID { get; set; }
        public string NombreHotel { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public int CapacidadMaxima { get; set; }
        public string Ubicacion { get; set; }
        public decimal? PrecioPorHora { get; set; }
        public bool EstaActivo { get; set; }
    }
}