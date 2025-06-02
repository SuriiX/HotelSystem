// File: ~/IdentityModels/ApplicationUser.cs
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelAuroraDreams.Api_Framework.IdentityModels
{
    public class ApplicationUser : IdentityUser
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

        [StringLength(200)]
        public string Direccion { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public DateTime FechaContratacion { get; set; }

        [Column(TypeName = "decimal")] // Dejamos que EF6/SQL Server definan precisión por defecto (18,2)
        public decimal Salario { get; set; }

        [StringLength(20)]
        public string Estado { get; set; }

        public ApplicationUser()
        {
            FechaContratacion = DateTime.UtcNow;
            Estado = "activo";
        }
    }
}