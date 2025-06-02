using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace HotelAuroraDreams.Api_Framework.Models.DTO
{
    public class EmpleadoUpdateBindingModel
    {
        [Required]
        [StringLength(100)]
        public string Nombre { get; set; }

        [Required]
        [StringLength(100)]
        public string Apellido { get; set; }
        public int? HotelID { get; set; }
        public int? CargoID { get; set; }

        [StringLength(20)]
        public string TipoDocumento { get; set; }

        [StringLength(20)]
        public string NumeroDocumento { get; set; }

        [Phone]
        [StringLength(20)]
        public string PhoneNumber { get; set; }

        [StringLength(200)]
        public string Direccion { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public DateTime FechaContratacion { get; set; }

        [DataType(DataType.Currency)]
        public decimal Salario { get; set; }

        [StringLength(20)]
        public string Estado { get; set; }
        public List<string> Roles { get; set; }
    }
}