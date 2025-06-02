// File: ~/Models/DTO/UserInfoViewModel.cs
namespace HotelAuroraDreams.Api_Framework.Models.DTO
{
    public class UserInfoViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public int? HotelID { get; set; }
        public int? CargoID { get; set; }
    }
}