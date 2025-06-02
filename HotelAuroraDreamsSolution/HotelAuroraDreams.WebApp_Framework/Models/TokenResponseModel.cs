// File: ~/Models/TokenResponseModel.cs
namespace HotelAuroraDreams.WebApp_Framework.Models
{
    public class TokenResponseModel
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
        public string userName { get; set; }
        public string nombreCompleto { get; set; }
    }
}