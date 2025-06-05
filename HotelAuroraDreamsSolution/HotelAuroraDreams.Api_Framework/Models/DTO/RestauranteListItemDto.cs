// File: ~/Models/DTO/RestauranteListItemDto.cs (API Project)
namespace HotelAuroraDreams.Api_Framework.Models.DTO
{
    public class RestauranteListItemDto
    {
        public int RestauranteID { get; set; }
        public string Nombre { get; set; }
        public int HotelID { get; set; }
        public string NombreHotel { get; set; }
    }
}