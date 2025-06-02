// File: ~/Models/ClienteViewModel.cs (en HotelAuroraDreams.WebApp_Framework)
using System;

namespace HotelAuroraDreams.WebApp_Framework.Models
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
        public string NombreCiudadResidencia { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public DateTime FechaRegistro { get; set; }
    }
}