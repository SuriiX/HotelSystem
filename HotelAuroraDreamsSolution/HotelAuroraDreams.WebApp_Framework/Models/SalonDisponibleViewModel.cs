// File: ~/Models/SalonDisponibleViewModel.cs (WebApp)
namespace HotelAuroraDreams.WebApp_Framework.Models
{
    public class SalonDisponibleViewModel
    {
        public int SalonEventoID { get; set; }
        public string NombreSalon { get; set; }
        public bool Disponible { get; set; }
        public string Mensaje { get; set; }
        public int? CapacidadRestanteEstimada { get; set; }
    }
}