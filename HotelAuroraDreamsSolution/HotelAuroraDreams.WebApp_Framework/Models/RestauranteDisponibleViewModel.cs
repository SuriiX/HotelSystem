// File: ~/Models/RestauranteDisponibleViewModel.cs (WebApp)
namespace HotelAuroraDreams.WebApp_Framework.Models
{
    public class RestauranteDisponibleViewModel
    {
        public int RestauranteID { get; set; }
        public string NombreRestaurante { get; set; }
        public bool Disponible { get; set; }
        public string Mensaje { get; set; }
        public int? CapacidadRestanteEstimada { get; set; }
    }
}