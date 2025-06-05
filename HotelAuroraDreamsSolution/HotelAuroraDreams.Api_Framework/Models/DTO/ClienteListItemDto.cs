namespace HotelAuroraDreams.Api_Framework.Models.DTO
{
    public class ClienteListItemDto // Para la búsqueda y listas simples
    {
        public int ClienteID { get; set; }
        public string NombreCompleto { get; set; } // Ej: "Perez, Juan (Doc: 12345)"
        public string NumeroDocumento { get; set; }
        public string Email { get; set; } // Opcional, pero útil para mostrar
    }
}