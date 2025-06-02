using System;
using System.ComponentModel.DataAnnotations;

namespace HotelAuroraDreams.Api_Framework.Models.DTO
{
    public class ReservaUpdateBindingModel
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "El número de huéspedes debe ser al menos 1.")]
        public int NumeroHuespedes { get; set; }

        public string Notas { get; set; }

        [Required]
        [StringLength(20)]
        public string Estado { get; set; } // Ej: Confirmada, Completada, No Show
    }
}