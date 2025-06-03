// File: ~/Models/DTO/RestauranteDisponibleViewModel.cs (API Project)
namespace HotelAuroraDreams.Api_Framework.Models.DTO
{
    public class RestauranteDisponibleViewModel
    {
        public int RestauranteID { get; set; }
        public string NombreRestaurante { get; set; }
        public bool Disponible { get; set; }
        public string Mensaje { get; set; }
        public int? CapacidadRestanteEstimada { get; set; } // Opcional
    }
}