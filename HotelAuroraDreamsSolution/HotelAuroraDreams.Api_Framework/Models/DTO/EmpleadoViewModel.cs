using System;
using System.Collections.Generic;

namespace HotelAuroraDreams.Api_Framework.Models.DTO
{
    public class EmpleadoViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public int? HotelID { get; set; }
        public string NombreHotel { get; set; }
        public int? CargoID { get; set; }
        public string NombreCargo { get; set; }
        public string TipoDocumento { get; set; }
        public string NumeroDocumento { get; set; }
        public string Direccion { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public DateTime FechaContratacion { get; set; }
        public decimal Salario { get; set; }
        public string Estado { get; set; }
        public IList<string> Roles { get; set; }
    }
}