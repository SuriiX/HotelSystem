// File: ~/Models/DTO/SalonDisponibleViewModel.cs (API Project)
namespace HotelAuroraDreams.Api_Framework.Models.DTO
{
    public class SalonDisponibleViewModel
    {
        public int SalonEventoID { get; set; }
        public string NombreSalon { get; set; }
        public bool Disponible { get; set; }
        public string Mensaje { get; set; } // Ej. "Disponible" o "No disponible: Ya reservado para [NombreEvento]"
    }
}