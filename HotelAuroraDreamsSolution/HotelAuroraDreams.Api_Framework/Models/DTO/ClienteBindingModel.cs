using System;
using System.ComponentModel.DataAnnotations;

namespace HotelAuroraDreams.Api_Framework.Models.DTO
{
    public class ClienteBindingModel
    {
        [Required]
        [StringLength(100)]
        public string Nombre { get; set; }

        [Required]
        [StringLength(100)]
        public string Apellido { get; set; }

        [Required]
        [StringLength(20)]
        public string TipoDocumento { get; set; } // Validar contra CK_Cliente_TipoDocumento en la BD

        [Required]
        [StringLength(20)]
        public string NumeroDocumento { get; set; }

        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        [StringLength(20)]
        public string Telefono { get; set; }

        [StringLength(200)]
        public string Direccion { get; set; }

        public int? CiudadResidenciaID { get; set; }
        public DateTime? FechaNacimiento { get; set; }
    }
}