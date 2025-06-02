using System;

namespace HotelAuroraDreams.Api_Framework.Models.DTO
{
    public class ClienteViewModel
    {
        public int ClienteID { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string TipoDocumento { get; set; }
        public string NumeroDocumento { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }
        public string Direccion { get; set; }
        public int? CiudadResidenciaID { get; set; }
        public string NombreCiudadResidencia { get; set; } // Para mostrar
        public DateTime? FechaNacimiento { get; set; }
        public DateTime FechaRegistro { get; set; }
    }
}