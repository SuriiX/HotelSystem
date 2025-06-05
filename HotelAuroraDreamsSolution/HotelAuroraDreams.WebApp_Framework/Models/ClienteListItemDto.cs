using System;

namespace HotelAuroraDreams.WebApp_Framework.Models
{
    [Serializable]
    public class ClienteListItemDto
    {
        public int ClienteID { get; set; }
        public string NombreCompleto { get; set; }
        public string NumeroDocumento { get; set; }
        public string Email { get; set; }
    }
}