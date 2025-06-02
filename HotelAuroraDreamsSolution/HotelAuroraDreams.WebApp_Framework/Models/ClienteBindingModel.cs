
using System;
using System.ComponentModel.DataAnnotations; 

namespace HotelAuroraDreams.WebApp_Framework.Models
{
    public class ClienteBindingModel
    {
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string TipoDocumento { get; set; }
        public string NumeroDocumento { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }
        public string Direccion { get; set; }
        public int? CiudadResidenciaID { get; set; } 
        public DateTime? FechaNacimiento { get; set; }
    }
}